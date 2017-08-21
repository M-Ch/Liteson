using System.IO;

namespace Liteson
{
	public static class JsonConvert
	{
		private static readonly TypeCatalog Catalog = new TypeCatalog();
		private static readonly WriterSettings DefaultSettings = new WriterSettings();

		public static string Serialize(object value) => Serialize(value, DefaultSettings);

		public static T Deserialize<T>(string value)
		{
			var descriptor = Catalog.GetDescriptor(typeof(T));
			var reader = new JsonReader(value);
			return (T)descriptor.Reader(reader);
		}

		public static string Serialize(object value, WriterSettings settings)
		{
			if (value == null)
				return "null";

			var descriptor = Catalog.GetDescriptor(value.GetType());
			var sw = new StringWriter();
			var context = new SerializationContext
			{
				Writer = new JsonWriter(sw, settings),
				Depth = 10 //todo
			};

			ObjectWriter.Write(value, context, descriptor.SerializationPlan);
			return sw.ToString();
		}
	}
}