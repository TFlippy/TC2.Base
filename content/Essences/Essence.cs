
namespace TC2.Base.Components
{
	public static partial class Essence
	{
		[Query]
		public delegate void GetAllSurgesQuery(ISystem.Info info, Entity entity, [Source.Owned] in EssenceNode.Data surge, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body);

		public enum Type: uint
		{
			Undefined,

			Energy,
			Motion,
			Life,
			Radiance,
			Cognition
		}

		// TODO: Just for debugging (until essences get implemented properly)
		public static Color32BGRA GetColor(Essence.Type type) => type switch
		{
			Essence.Type.Undefined => new Color32BGRA(255, 255, 255, 255),

			Essence.Type.Energy => new Color32BGRA(255, 255, 180, 0),
			Essence.Type.Motion => new Color32BGRA(255, 0, 255, 255),
			Essence.Type.Life => new Color32BGRA(255, 50, 255, 0),
			Essence.Type.Radiance => new Color32BGRA(255, 255, 255, 255),
			Essence.Type.Cognition => new Color32BGRA(255, 130, 0, 255),

			_ => new Color32BGRA(255, 255, 255, 255)
		};
	}
}
