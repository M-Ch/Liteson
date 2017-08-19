using System;
using System.Collections.Generic;
using System.Linq;

namespace Liteson
{
	internal class TypeCatalog
	{
		private Dictionary<Type, TypeDescriptor> _descriptors = new List<TypeDescriptor>
		{
			ForPrimitive<byte>((v, c) => c.Writer.Write((byte) v)),
			ForPrimitive<sbyte>((v, c) => c.Writer.Write((sbyte) v)),
			ForPrimitive<char>((v, c) => c.Writer.Write((char) v)),
			ForPrimitive<short>((v, c) => c.Writer.Write((short) v)),
			ForPrimitive<ushort>((v, c) => c.Writer.Write((ushort) v)),
			ForPrimitive<int>((v, c) => c.Writer.Write((int) v)),
			ForPrimitive<uint>((v, c) => c.Writer.Write((uint) v)),
			ForPrimitive<long>((v, c) => c.Writer.Write((long) v)),
			ForPrimitive<ulong>((v, c) => c.Writer.Write((ulong) v)),
			ForPrimitive<float>((v, c) => c.Writer.Write((float) v)),
			ForPrimitive<double>((v, c) => c.Writer.Write((double) v)),
			ForPrimitive<decimal>((v, c) => c.Writer.Write((decimal) v)),
			ForPrimitive<byte[]>((v, c) => c.Writer.Write((byte[]) v)),
			ForPrimitive<string>((v, c) => c.Writer.Write((string) v)),
			ForPrimitive<object>((v, c) => c.Writer.Write(v.ToString())),
			ForPrimitive<DateTime>((v, c) => c.Writer.Write((DateTime) v)),
			ForPrimitive<TimeSpan>((v, c) => c.Writer.Write((TimeSpan) v)),
			ForPrimitive<Guid>((v, c) => c.Writer.Write((Guid) v))
		}.ToDictionary(i => i.Type);

		public TypeDescriptor GetDescriptor(Type type)
		{
			if (_descriptors.TryGetValue(type, out var value))
				return value;

			var descriptors = new Dictionary<Type, TypeDescriptor>();
			TypeDescriptor DescriptorSource(Type t) => _descriptors.GetIfPresent(t) ?? CreateDescriptorTree(t, descriptors, DescriptorSource);

			CreateDescriptorTree(type, descriptors, DescriptorSource);
			foreach (var existing in _descriptors)
				descriptors[existing.Key] = existing.Value;

			_descriptors = descriptors;
			return descriptors[type];
		}

		private static TypeDescriptor CreateDescriptorTree(Type root, Dictionary<Type, TypeDescriptor> subDescriptors, Func<Type, TypeDescriptor> descriptorSource)
		{
			var descriptor = new TypeDescriptor
			{
				Type = root
			};
			subDescriptors.Add(root, descriptor);
			descriptor.SerializationPlan = SerializationPlan.ForType(root, descriptorSource);

			return descriptor;
		}

		private static TypeDescriptor ForPrimitive<T>(Action<object, SerializationContext> serializer) => new TypeDescriptor
		{
			Type = typeof(T),
			SerializationPlan = new SerializationPlan
			{
				Steps = new[] {serializer}
			}
		};
	}
}