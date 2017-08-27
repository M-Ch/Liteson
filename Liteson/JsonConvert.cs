using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Liteson
{
	public static class JsonConvert
	{
		private static readonly TypeCatalog Catalog = new TypeCatalog();
		private static readonly SerializationSettings DefaultSettings = new SerializationSettings();

		public static string Serialize(object value) => Serialize(value, DefaultSettings);
		public static T Deserialize<T>(string value) => Deserialize<T>(value, DefaultSettings);

		public static T Deserialize<T>(string value, SerializationSettings settings)
		{
			var descriptor = Catalog.GetDescriptor(typeof(T), OptionsFromSettings(settings));
			var context = new DeserializationContext
			{
				Catalog = Catalog,
				TypeSelectors = settings.TypeSelectors.Count > 0 ? settings.TypeSelectors.ToDictionary(i => i.Key, i => i.Value) : null,
				Reader = new JsonReader(value)
			};
			return (T)descriptor.Reader(context);
		}

		public static string Serialize(object value, SerializationSettings settings)
		{
			if (value == null)
				return "null";

			var descriptor = Catalog.GetDescriptor(value.GetType(), OptionsFromSettings(settings));
			var sw = new StringWriter();
			var context = new SerializationContext
			{
				Writer = new JsonWriter(sw, settings),
				Depth = 10 //todo
			};

			descriptor.Writer(value, context);
			return sw.ToString();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static TypeOptions OptionsFromSettings(SerializationSettings settings) =>
			(settings.CamelCaseNames ? TypeOptions.CamelCase : TypeOptions.None) |
			(settings.EnumsToStrings ? TypeOptions.EnumsToStrings : TypeOptions.None);
	}
}