
namespace TC2.Base.Components
{
	[Asset.Hjson(prefix: "essence.", capacity_world: 0, capacity_region: 0, capacity_local: 0, order: -30)]
	public interface IEssence: IAsset2<IEssence, IEssence.Data>
	{
		[Flags]
		public enum Flags: uint
		{
			None = 0,
		}

		[Serializable]
		public struct Data
		{
			[Save.Force] public string name;
			[Save.Force, Save.MultiLine] public string desc;

			[Save.NewLine]
			[Save.Force] public IEssence.Flags flags;
			[Save.Force] public Essence.Type type_tmp;

			[Save.NewLine]
			[Save.Force] public ColorBGRA color_emit = new ColorBGRA(1.00f, 1.00f, 1.00f, 1.00f);

			//[Save.NewLine]
			//[Save.Force, Obsolete] public float force_emit;
			//[Save.Force, Obsolete] public float heat_emit;

			[Save.Force] public float emit_force;
			[Save.Force] public Power emit_power_thermal;
			[Save.Force] public Power emit_power_kinetic;
			[Save.Force] public Power emit_power_radiant;
			[Save.Force] public Power emit_power_electric;
			[Save.Force] public Power emit_power_magnetic;

			[Save.NewLine]
			[Save.Force] public Sound.Handle sound_emit_loop;
			[Save.Force] public Sound.Handle sound_emit_pulser_loop;
			[Save.Force] public Sound.Handle sound_emit_stressor_loop;
			[Save.Force] public Sound.Handle sound_emit_impactor_loop;
			[Save.Force] public Sound.Handle sound_emit_cycler_loop;
			[Save.Force] public Sound.Handle sound_emit_oscillator_loop;
			[Save.Force] public Sound.Handle sound_emit_ambient_loop;
			[Save.Force] public Sound.Handle sound_drain_loop;
			[Save.Force] public Sound.Handle sound_collapse;
			[Save.Force] public Sound.Handle sound_zap;
			[Save.Force] public Sound.Handle sound_blast;
			[Save.Force] public Sound.Handle sound_impulse;
			[Save.Force] public Sound.Handle sound_impact;

			[Save.NewLine]
			[Save.Force] public Prefab.Handle h_prefab_node;
			[Save.Force] public IMaterial.Handle h_material_pellet;

			public Data()
			{

			}
		}
	}

	public static partial class Essence
	{
		public static class Emitter
		{
			public enum Type: byte
			{
				Undefined,

				[Name("Ambient", desc: "Passive emitter activated by surrounding environment.")]
				Ambient,
				[Name("Impactor", desc: "Simple discrete emitter activated by kinetic impulses.")]
				Impactor,
				[Name("Cycler", desc: "Reciprocating emitter activated by a pair of pellets interacting with eachother.")]
				Cycler,
				[Name("Pulser", desc: "Adjustable discrete emitter activated by short electrical pulses.")]
				Pulser,
				[Name("Stressor", desc: "Adjustable continuous emitter activated by compressive force.")]
				Stressor,
				[Name("Oscillator", desc: "Variable-frequency emitter activated by alternating electrical current.")]
				Oscillator,
				[Name("Fragmenter", desc: "Powerful single-use emitter activated by shattering the pellet.")]
				Fragmenter,
				[Name("Projector", desc: "Experimental emitter array capable of projecting effects of essences onto distant surfaces.")]
				Projector,

			}

			[Flags]
			public enum Flags: ushort
			{
				None = 0,

				Show_GUI = 1 << 0,
				Allow_Edit_Rate = 1 << 1,
				Allow_Edit_Frequency = 1 << 2
			}

			[IComponent.Data(Net.SendType.Reliable, region_only: true)]
			public partial struct Data: IComponent
			{
				public Essence.Emitter.Type type;
				public Essence.Emitter.Flags flags;

				public float frequency = 0.00f;

				[Editor.Slider.Clamped(-float.Tau, +float.Tau, float.Pi / 16.00f)]
				public float rotation;
				[Editor.Picker.Position(true)]
				public Vector2 offset;

				public float rate_speed = 0.10f;
				public float rate_max;
				public float rate_target;
				[Asset.Ignore] public float rate_current;

				public Data()
				{

				}
			}
		}

		//public interface IPowered: IComponent
		//{
		//	public float EssenceAvailable { get; set; }
		//	public float EssenceRate { get; set; }

		//	public static void UpdateInventory<T>(ref T powered, ref Inventory1.Data inventory) where T : unmanaged, Essence.IPowered
		//	{
		//		powered.EssenceAvailable = inventory.resource.quantity * essence_per_pellet;
		//	}
		//}

#if SERVER
		[ChatCommand.Region("essenceboom", "", admin: true)]
		public static void EssenceBoomCommand(ref ChatCommand.Context context, string identifier, float amount)
		{
			ref var region = ref context.GetRegion();
			if (region.IsNotNull())
			{
				var random = XorRandom.New(true);
				//EssenceNode.Collapse(ref region, ref random, identifier, default, context.GetPlayer().control.mouse.position, amount);
				Essence.SpawnCollapsingNode(ref region, identifier, amount, context.GetPlayerData().control.mouse.position, full_collapse: true);
			}
		}

		public static void SpawnCollapsingNode(ref Region.Data region, IEssence.Handle h_essence, float amount, Vector2 position, bool full_collapse = true)
		{
			ref var essence_data = ref h_essence.GetData();
			if (essence_data.IsNotNull())
			{
				var prefab = essence_data.h_prefab_node;
				if (prefab.id != 0)
				{
					if (amount >= 1.00f)
					{
						region.SpawnPrefab(prefab, position).ContinueWith(x =>
						{
							ref var essence_node = ref x.GetComponent<EssenceNode.Data>();
							if (essence_node.IsNotNull())
							{
								essence_node.h_essence = h_essence;
								essence_node.amount = amount;
								essence_node.stability = 0.00f;
								essence_node.volatility = 1.00f;
								essence_node.flags.AddFlag(EssenceNode.Flags.Full_Collapse, full_collapse);

								essence_node.Sync(x, true);
							}
						});
					}
				}
			}
		}
#endif

		[Query(ISystem.Scope.Region)]
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

		public static readonly Dictionary<IMaterial.Handle, IEssence.Handle> material_to_essence = new();
		public static readonly Dictionary<IEssence.Handle, IMaterial.Handle> essence_to_material = new();

		public const float essence_per_pellet = 10.00f;
		//public const float force_per_motion_essence = 2000.00f;

		//public static float GetEssenceForce(Essence.Type type) => type switch
		//{
		//	Essence.Type.Heat => 1400.00f,
		//	Essence.Type.Motion => 2000.00f,
		//	Essence.Type.Life => 400.00f,
		//	Essence.Type.Radiance => 800.00f,
		//	Essence.Type.Cognition => 200.00f,
		//	Essence.Type.Electricity => -800.00f,
		//	Essence.Type.Failure => -200.00f,

		//	_ => default
		//};

		//public static float GetEssenceHeat(Essence.Type type) => type switch
		//{
		//	Essence.Type.Heat => 20.00f,
		//	Essence.Type.Motion => 4.00f,
		//	Essence.Type.Life => 1.00f,
		//	Essence.Type.Radiance => 10.00f,
		//	Essence.Type.Cognition => -2.00f,
		//	Essence.Type.Electricity => 8.00f,
		//	Essence.Type.Failure => -5.00f,

		//	_ => default
		//};

		//public static Sound.Handle GetEssenceEmitSound(Essence.Type type) => type switch
		//{
		//	Essence.Type.Heat => "essence.emit.heat.loop.00",
		//	Essence.Type.Motion => "essence.emit.motion.loop.00",
		//	Essence.Type.Life => "essence.emit.life.loop.00",
		//	Essence.Type.Radiance => "essence.emit.cognition.loop.00",
		//	Essence.Type.Cognition => "essence.emit.cognition.loop.00",
		//	Essence.Type.Electricity => "essence.emit.cognition.loop.00",
		//	Essence.Type.Failure => "essence.emit.cognition.loop.00",

		//	_ => default
		//};

		//public static void Init()
		//{
		//	var materials = NetRegistry<Material.Definition>.GetAll();
		//	material_to_essence = new Essence.Type[materials.Length];

		//	for (var i = 0; i < material_to_essence.Length; i++)
		//	{
		//		ref var material = ref materials[i];
		//		if (material.flags.HasAny(Material.Flags.Essence))
		//		{
		//			var identifier = material.identifier.ToString();

		//			var char_index = identifier.LastIndexOf('.');
		//			if (char_index != -1)
		//			{
		//				var enum_name = identifier.Substring(char_index + 1);
		//				if (Enum.TryParse<Essence.Type>(enum_name, ignoreCase: true, out var essence_type))
		//				{
		//					material_to_essence[i] = essence_type;
		//				}
		//			}
		//		}
		//	}
		//}

		public static IEssence.Handle GetEssenceType(in this Resource.Data resource) => Essence.GetEssenceType(resource.material.id);
		public static IEssence.Handle GetEssenceType(this IMaterial.Handle material)
		{
			material_to_essence.TryGetValue(material, out var essence_type);
			return essence_type;

			//return material.id < material_to_essence.Length ? material_to_essence[material.id] : default;
		}

		//public static Prefab.Handle GetEssencePrefab(Essence.Type type) => type switch
		//{
		//	Essence.Type.Heat => "essence_node.heat",
		//	Essence.Type.Motion => "essence_node.motion",
		//	Essence.Type.Life => "essence_node.life",
		//	Essence.Type.Radiance => "essence_node.radiance",
		//	Essence.Type.Cognition => "essence_node.cognition",
		//	Essence.Type.Electricity => "essence_node.electricity",
		//	Essence.Type.Failure => "essence_node.failure",

		//	_ => default
		//};

		//public static IMaterial.Handle GetEssenceMaterial(Essence.Type type)
		//{
		//	essence_to_material.TryGetValue(type, out var material);
		//	return material;
		//}

		//// TODO: Just for debugging (until essences get implemented properly)
		//public static ColorBGRA GetColor(Essence.Type type) => type switch
		//{
		//	Essence.Type.Undefined => new Color32BGRA(255, 255, 255, 255),

		//	Essence.Type.Heat => new Color32BGRA(255, 255, 180, 0),
		//	Essence.Type.Motion => new Color32BGRA(255, 0, 255, 255),
		//	Essence.Type.Life => new Color32BGRA(255, 50, 255, 0),
		//	Essence.Type.Radiance => new Color32BGRA(255, 255, 255, 255),
		//	Essence.Type.Cognition => new Color32BGRA(255, 130, 0, 255),
		//	Essence.Type.Electricity => new Color32BGRA(255, 165, 205, 255),
		//	Essence.Type.Failure => new Color32BGRA(255, 165, 130, 90),

		//	_ => new Color32BGRA(255, 255, 255, 255)
		//};


		//[ISystem.Update(ISystem.Mode.Single, ISystem.Scope.Region)]
		//public static void UpdateInventory<T>(ISystem.Info info, Entity entity,
		//[Source.Owned] ref T powered, [Source.Owned, Pair.Component<>] ref Inventory1.Data inventory) where T : unmanaged, IComponent, Essence.IPowered
		//{

		//}
	}
}
