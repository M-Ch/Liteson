namespace JsonSad
{
	internal static class ObjectWriter
	{
		public static void Write(object item, SerializationContext context, SerializationPlan plan)
		{
			if (item == null)
			{
				context.Writer.WriteNull();
				return;
			}

			foreach (var step in plan.Steps)
				step(item, context);
		}
	}
}