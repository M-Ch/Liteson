using System;
using System.Collections.Generic;
using System.Linq;

namespace Liteson
{
	internal class TypeCatalog
	{
		private Dictionary<Type, TypeDescriptor> _descriptors = new List<TypeDescriptor>
		{
			ForPrimitive<bool>((v, c) => c.Writer.Write((bool) v), r => ParsedReading.ReadBool(r)),
			ForPrimitive<byte>((v, c) => c.Writer.Write((byte) v), r => ParsedReading.ReadByte(r)),
			ForPrimitive<sbyte>((v, c) => c.Writer.Write((sbyte) v), r => ParsedReading.ReadSByte(r)),
			ForPrimitive<char>((v, c) => c.Writer.Write((char) v), r => ParsedReading.ReadChar(r)),
			ForPrimitive<short>((v, c) => c.Writer.Write((short) v), r => ParsedReading.ReadShort(r)),
			ForPrimitive<ushort>((v, c) => c.Writer.Write((ushort) v), r => ParsedReading.ReadUShort(r)),
			ForPrimitive<int>((v, c) => c.Writer.Write((int) v), r => ParsedReading.ReadInt(r)),
			ForPrimitive<uint>((v, c) => c.Writer.Write((uint) v), r => ParsedReading.ReadUInt(r)),
			ForPrimitive<long>((v, c) => c.Writer.Write((long) v), r => ParsedReading.ReadLong(r)),
			ForPrimitive<ulong>((v, c) => c.Writer.Write((ulong) v), r => ParsedReading.ReadULong(r)),
			ForPrimitive<float>((v, c) => c.Writer.Write((float) v), r => ParsedReading.ReadFloat(r)),
			ForPrimitive<double>((v, c) => c.Writer.Write((double) v), r => ParsedReading.ReadDouble(r)),
			ForPrimitive<decimal>((v, c) => c.Writer.Write((decimal) v), r => ParsedReading.ReadDecimal(r)),
			ForPrimitive<byte[]>((v, c) => c.Writer.Write((byte[]) v), ParsedReading.ReadByteArray),
			ForPrimitive<string>((v, c) => c.Writer.Write((string) v), ParsedReading.ReadString),
			ForPrimitive<object>((v, c) => c.Writer.Write(v.ToString()), null), //todo, can be anything
			ForPrimitive<DateTime>((v, c) => c.Writer.Write((DateTime) v), r => ParsedReading.ReadDateTime(r)),
			ForPrimitive<TimeSpan>((v, c) => c.Writer.Write((TimeSpan) v), r => ParsedReading.ReadTimeSpan(r)),
			ForPrimitive<Guid>((v, c) => c.Writer.Write((Guid) v), r => ParsedReading.ReadGuid(r))
		}.ToDictionary(i => i.Type);

		public TypeDescriptor GetDescriptor(Type type)
		{
			if (_descriptors.TryGetValue(type, out var value))
				return value;

			//In case of multi-threaded concurrent initialization only values produced by one of concurrent threads will be put into final _descriptors dictionary.
			//Other thread's work result will be discarded but it's fine because it will happen only once. 
			//This way we don't need to use any locks or thread-safe collections all the time.
			var descriptors = new Dictionary<Type, TypeDescriptor>();
			TypeDescriptor DescriptorSource(Type t) => _descriptors.TryGetValue(t, out var descriptor) 
				? descriptor 
				: descriptors.TryGetValue(t, out var local) ? local : CreateDescriptorTree(t, descriptors, DescriptorSource);

			var result = CreateDescriptorTree(type, descriptors, DescriptorSource);
			foreach (var existing in _descriptors)
				descriptors[existing.Key] = existing.Value;

			_descriptors = descriptors;
			return result;
		}

		private static TypeDescriptor CreateDescriptorTree(Type root, IDictionary<Type, TypeDescriptor> subDescriptors, Func<Type, TypeDescriptor> descriptorSource)
		{
			var descriptor = new TypeDescriptor { Type = root };
			subDescriptors.Add(root, descriptor);
			descriptor.SerializationPlan = SerializationPlan.ForType(root, descriptorSource);
			descriptor.Reader = TypeReader.ForType(root, descriptorSource);
			return descriptor;
		}

		private static TypeDescriptor ForPrimitive<T>(Action<object, SerializationContext> writer, Func<JsonReader, object> reader) => new TypeDescriptor
		{
			Type = typeof(T),
			Reader = reader,
			SerializationPlan = new SerializationPlan
			{
				Steps = new[] {writer}
			}
		};
	}
}