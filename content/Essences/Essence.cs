
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

		[Flags]
		public enum Tags: uint
		{
			None = 0,

			Kinetic = 1 << 0,
			Thermal = 1 << 1,
			Primary = 1 << 2,
			Secondary = 1 << 3,
			Tertiary = 1 << 4,
			Violent = 1 << 5,
			Destructive = 1 << 6,
			Illegal = 1 << 7,
			Rare = 1 << 8,
			Physical = 1 << 9,
			Abstract = 1 << 10,
		}

		[FixedAddressValueType] public static FixedArray256<IEssence.Handle> material_to_essence;
		[FixedAddressValueType] public static FixedArray256<IMaterial.Handle> essence_to_material;

		[FixedAddressValueType] public static FixedArray256<IEssence.Flags> essence_to_flags;
		[FixedAddressValueType] public static FixedArray256<IEssence.Tags> essence_to_tags;

		[FixedAddressValueType] public static FixedArray256<ColorBGRA> essence_to_color_emit;

		[FixedAddressValueType] public static FixedArray256<float> essence_to_stability_mult;
		[FixedAddressValueType] public static FixedArray256<float> essence_to_discharge_mult;

		[FixedAddressValueType] public static FixedArray256<float> essence_to_emit_force;

		[FixedAddressValueType] public static FixedArray256<Power> essence_to_emit_power_thermal;
		[FixedAddressValueType] public static FixedArray256<Power> essence_to_emit_power_kinetic;
		[FixedAddressValueType] public static FixedArray256<Power> essence_to_emit_power_radiant;
		[FixedAddressValueType] public static FixedArray256<Power> essence_to_emit_power_electric;
		[FixedAddressValueType] public static FixedArray256<Power> essence_to_emit_power_magnetic;

		static void IAsset2<IEssence, IEssence.Data>.OnRefresh(IEssence.Definition definition)
		{
			ref var data = ref definition.GetData();

			var identifier = definition.identifier;
			var h_essence = definition.GetHandle();
			var index = (nuint)h_essence.id;

			var identifier_material = $"pellet.{identifier}";
			if (Assert.Check(IMaterial.Handle.TryParse(identifier_material, out var h_material), Assert.Level.Warn))
			{
				essence_to_material[index] = h_material;
				material_to_essence[h_material.id] = h_essence;
			}

			IEssence.essence_to_flags[index] = data.flags;
			IEssence.essence_to_tags[index] = data.tags;

			IEssence.essence_to_color_emit[index] = data.color_emit;

			IEssence.essence_to_stability_mult[index] = data.stability_mult;
			IEssence.essence_to_discharge_mult[index] = data.discharge_mult;

			IEssence.essence_to_emit_force[index] = data.emit_force;

			IEssence.essence_to_emit_power_thermal[index] = data.emit_power_thermal;
			IEssence.essence_to_emit_power_kinetic[index] = data.emit_power_kinetic;
			IEssence.essence_to_emit_power_radiant[index] = data.emit_power_radiant;
			IEssence.essence_to_emit_power_electric[index] = data.emit_power_electric;
			IEssence.essence_to_emit_power_magnetic[index] = data.emit_power_magnetic;
		}

		public struct Data(): IName, IDescription
		{
			[Save.Force] public required string name;
			[Save.Force, Save.MultiLine] public required string desc;

			[Save.NewLine]
			[Save.MultiLine] public string lore;

			[Save.NewLine]
			[Save.Force] public required IEssence.Flags flags;
			[Save.Force] public required IEssence.Tags tags;
			[Obsolete] public Essence.Type type_tmp;

			[Save.NewLine]
			[Save.Force] public required ColorBGRA color_emit;

			//[Save.NewLine]
			//[Save.Force, Obsolete] public float force_emit;
			//[Save.Force, Obsolete] public float heat_emit;

			[Save.NewLine]
			[Save.Force, Editor.Slider.Clamped(0.00f, 4.00f, snap: 0.01f, sensitivity: 0.01f)] public required float stability_mult = 1.00f;
			[Save.Force, Editor.Slider.Clamped(0.00f, 4.00f, snap: 0.01f, sensitivity: 0.01f)] public required float discharge_mult = 1.00f;

			[Save.NewLine]
			[Save.Force] public required float emit_force;
			[Save.Force] public required Power emit_power_thermal;
			[Save.Force] public required Power emit_power_kinetic;
			[Save.Force] public required Power emit_power_radiant;
			[Save.Force] public required Power emit_power_electric;
			[Save.Force] public required Power emit_power_magnetic;

			[Save.NewLine]
			[Obsolete] public Sound.Handle sound_emit_loop;
			[Obsolete] public Sound.Handle sound_drain_loop;

			[Save.NewLine]
			[Save.Force] public Sound.Handle sound_emit_pulser_loop;
			[Save.Force] public Sound.Handle sound_emit_stressor_loop;
			[Save.Force] public Sound.Handle sound_emit_impactor_loop;
			[Save.Force] public Sound.Handle sound_emit_cycler_loop;
			[Save.Force] public Sound.Handle sound_emit_oscillator_loop;
			[Save.Force] public Sound.Handle sound_emit_ambient_loop;
			[Save.Force] public Sound.Handle sound_emit_projector_loop;

			[Save.NewLine]
			[Save.Force] public Sound.Handle sound_collapse;
			[Save.Force] public Sound.Handle sound_zap;
			[Save.Force] public Sound.Handle sound_blast;
			[Save.Force] public Sound.Handle sound_impulse;
			[Save.Force] public Sound.Handle sound_impact;
			[Save.Force] public Sound.Handle sound_shatter;

			[Save.NewLine]
			[Save.Force] public required Prefab.Handle h_prefab_node;
			[Save.Force] public required IMaterial.Handle h_material_pellet;

			[Save.NewLine]
			[Save.Force] public IEvent.Info on_collapse;
			[Save.Force] public IEvent.Info on_failure;

			readonly ReadOnlySpan<char> IName.GetName() => this.name;
			readonly ReadOnlySpan<char> IName.GetShortName() => this.name;
			readonly ReadOnlySpan<char> IDescription.GetDescription() => this.desc;
		}
	}

	public static partial class Essence
	{
		public static float GetForce(this IEssence.Handle h_essence) => IEssence.essence_to_emit_force[h_essence.id];
		public static Power GetThermalPower(this IEssence.Handle h_essence) => IEssence.essence_to_emit_power_thermal[h_essence.id];
		public static Power GetKineticPower(this IEssence.Handle h_essence) => IEssence.essence_to_emit_power_kinetic[h_essence.id];
		public static Power GetRadiantPower(this IEssence.Handle h_essence) => IEssence.essence_to_emit_power_radiant[h_essence.id];
		public static Power GetElectricPower(this IEssence.Handle h_essence) => IEssence.essence_to_emit_power_electric[h_essence.id];
		public static Power GetMagneticPower(this IEssence.Handle h_essence) => IEssence.essence_to_emit_power_magnetic[h_essence.id];

		public static ColorBGRA GetEmitColor(this IEssence.Handle h_essence) => IEssence.essence_to_color_emit[h_essence.id];

		public static float GetStabilityMult(this IEssence.Handle h_essence) => IEssence.essence_to_stability_mult[h_essence.id];
		public static float GetDischargeMult(this IEssence.Handle h_essence) => IEssence.essence_to_discharge_mult[h_essence.id];

		public static float GetForce(in this Essence.Container.Data container) => container.GetEmittedEssenceAmount() * container.h_essence.GetForce();
		public static Power GetThermalPower(in this Essence.Container.Data container) => container.GetEmittedEssenceAmount() * container.h_essence.GetThermalPower();
		public static Power GetKineticPower(in this Essence.Container.Data container) => container.GetEmittedEssenceAmount() * container.h_essence.GetKineticPower();
		public static Power GetRadiantPower(in this Essence.Container.Data container) => container.GetEmittedEssenceAmount() * container.h_essence.GetRadiantPower();
		public static Power GetElectricPower(in this Essence.Container.Data container) => container.GetEmittedEssenceAmount() * container.h_essence.GetElectricPower();
		public static Power GetMagneticPower(in this Essence.Container.Data container) => container.GetEmittedEssenceAmount() * container.h_essence.GetMagneticPower();

		public static float GetEmittedEssenceAmount(in this Essence.Container.Data container)
		{
			return container.available * container.rate_current;
		}

		[IEvent.Data]
		public partial struct PulseEvent(): IEvent
		{
			public required IEssence.Handle h_essence;
			//public required Essence.Emitter.Type emitter_type;

			public required float amount;

			public required Vec2f pos;
			public required Vec2f dir;



		}

#if SERVER
		[ChatCommand.Region("essenceboom", "", admin: true)]
		public static void EssenceBoomCommand(ref ChatCommand.Context context, string identifier, float amount)
		{
			ref var region = ref context.GetRegion();
			if (region.IsNotNull())
			{
				var random = XorRandom.New(true);
				//EssenceNode.Collapse(ref region, ref random, identifier, default, context.GetPlayer().control.mouse.position, amount);
				Essence.SpawnCollapsingNode(region: ref region, h_essence: identifier, amount: amount, position: context.GetTargetPosition(), full_collapse: true);
			}
		}

		public static UniTask<Entity> SpawnCollapsingNode(ref Region.Data region, IEssence.Handle h_essence, float amount, Vector2 position, bool full_collapse = true)
		{
			ref var essence_data = ref h_essence.GetData();
			if (essence_data.IsNotNull())
			{
				var prefab = essence_data.h_prefab_node;
				if (prefab.id != 0)
				{
					if (amount >= 1.00f)
					{
						return region.SpawnPrefab(prefab, position).ContinueWith(ent_essence =>
						{
							ref var essence_node = ref ent_essence.GetComponent<EssenceNode.Data>();
							if (essence_node.IsNotNull())
							{
								essence_node.h_essence = h_essence;
								essence_node.amount = amount;
								essence_node.stability = 0.00f;
								essence_node.volatility = 1.00f;
								essence_node.flags.AddFlag(EssenceNode.Flags.Full_Collapse, full_collapse);

								essence_node.Sync(ent_essence, true);
							}

							return ent_essence;
						});
					}
				}
			}

			return default;
		}
#endif

		[Query(ISystem.Scope.Region)]
		public delegate void GetAllNodesQuery(ISystem.Info info, Entity entity,
			[Source.Owned] ref EssenceNode.Data essence_node, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body);

		// TODO: temporary, will be removed
		[Obsolete]
		public enum Type: uint
		{
			Undefined,

			Heat,
			Motion,
			Life,
			Radiance,
			Cognition,
			Electricity,
			Failure,
			Anger
		}

		public const float essence_per_pellet = 10.00f;
		public const float essence_per_pellet_inv = 1.00f / Essence.essence_per_pellet;

		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";
		public static readonly Texture.Handle texture_spark = "essence.spark";
		public static readonly Texture.Handle texture_glow = "essence.glow";
		public static readonly Texture.Handle texture_glow_transparent = "essence.glow.transparent";

		public static IEssence.Handle GetEssenceType(this Resource.Data resource) => Essence.GetEssenceType(resource.material);
		public static IEssence.Handle GetEssenceType(this IMaterial.Handle material) => IEssence.material_to_essence[(byte)material.id];
	}
}
