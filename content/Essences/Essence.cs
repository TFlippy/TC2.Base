﻿
namespace TC2.Base.Components
{
	public static partial class Essence
	{
		[Query]
		public delegate void GetAllNodesQuery(ISystem.Info info, Entity entity, [Source.Owned] ref EssenceNode.Data essence_node, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body);

		public enum Type: uint
		{
			Undefined,

			Heat,
			Motion,
			Life,
			Radiance,
			Cognition,
			Electricity,
			Failure
		}

		private static Essence.Type[] material_to_essence;

		public static void Init()
		{
			var materials = NetRegistry<Material.Definition>.GetAll();
			material_to_essence = new Essence.Type[materials.Length];

			for (var i = 0; i < material_to_essence.Length; i++)
			{
				ref var material = ref materials[i];
				if (material.flags.HasAny(Material.Flags.Essence))
				{
					var identifier = material.identifier.ToString();

					var char_index = identifier.LastIndexOf('.');
					if (char_index != -1)
					{
						var enum_name = identifier.Substring(char_index + 1);
						if (Enum.TryParse<Essence.Type>(enum_name, ignoreCase: true, out var essence_type))
						{
							material_to_essence[i] = essence_type;
						}
					}
				}
			}
		}

		public static Essence.Type GetEssenceType(in this Resource.Data resource) => Essence.GetEssenceType(resource.material.id);
		public static Essence.Type GetEssenceType(this Material.Handle material)
		{
			return material.id < material_to_essence.Length ? material_to_essence[material.id] : default;
		}

		public static Prefab.Handle GetEssencePrefab(Essence.Type type) => type switch
		{
			Essence.Type.Heat => "essence_node.heat",
			Essence.Type.Motion => "essence_node.motion",
			Essence.Type.Life => "essence_node.life",
			Essence.Type.Radiance => "essence_node.radiance",
			Essence.Type.Cognition => "essence_node.cognition",
			Essence.Type.Electricity => "essence_node.electricity",
			Essence.Type.Failure => "essence_node.failure",

			_ => default
		};

		// TODO: Just for debugging (until essences get implemented properly)
		public static ColorBGRA GetColor(Essence.Type type) => type switch
		{
			Essence.Type.Undefined => new Color32BGRA(255, 255, 255, 255),

			Essence.Type.Heat => new Color32BGRA(255, 255, 180, 0),
			Essence.Type.Motion => new Color32BGRA(255, 0, 255, 255),
			Essence.Type.Life => new Color32BGRA(255, 50, 255, 0),
			Essence.Type.Radiance => new Color32BGRA(255, 255, 255, 255),
			Essence.Type.Cognition => new Color32BGRA(255, 130, 0, 255),
			Essence.Type.Electricity => new Color32BGRA(255, 165, 205, 255),
			Essence.Type.Failure => new Color32BGRA(255, 165, 130, 90),

			_ => new Color32BGRA(255, 255, 255, 255)
		};
	}
}
