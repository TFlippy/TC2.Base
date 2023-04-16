using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class BaseMod
	{
		private static void RegisterExplosiveAugments(ref List<Augment.Definition> definitions)
		{
			definitions.Add(Augment.Definition.New<Explosive.Data>
			(
				identifier: "explosive.directed",
				category: "Explosive",
				name: "Directed Explosion",
				description: "Focuses the explosion into a smaller area.",

				apply_0: static (ref Augment.Context context, ref Explosive.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					//var radius_log = MathF.Log2(data.radius);

					data.radius *= 0.75f;
					data.power *= 1.25f;
					data.damage_terrain *= 1.25f;
					data.damage_entity *= 1.25f;
				}
			));

			definitions.Add(Augment.Definition.New<Explosive.Data>
			(
				identifier: "explosive.nitroglycerine",
				category: "Explosive",
				name: "Nitroglycerine Filler",
				description: "Replaces filler with nitroglycerine.",

				can_add: static (ref Augment.Context context, in Explosive.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					if (augments.HasAugment(handle)) return false;

					var material_nitroglycerine_handle = new IMaterial.Handle("nitroglycerine");
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && requirement.material.id != material_nitroglycerine_handle.id && material.flags.HasAny(Material.Flags.Explosive) && requirement.amount > 0.00f)
							{
								return true;
							}
						}
					}

					return false;
				},

				apply_0: static (ref Augment.Context context, ref Explosive.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					var material_nitroglycerine_handle = new IMaterial.Handle("nitroglycerine");
					ref var material_nitroglycerine = ref material_nitroglycerine_handle.GetData();
					if (material_nitroglycerine.IsNotNull())
					{
						data.flags |= Explosive.Flags.Any_Damage | Explosive.Flags.Explode_When_Primed;
						data.health_threshold = 0.70f;

						var has_any = false;

						foreach (ref var requirement in context.requirements_new)
						{
							if (requirement.type == Crafting.Requirement.Type.Resource)
							{
								ref var material = ref requirement.material.GetData();
								if (material.IsNotNull() && requirement.material.id != material_nitroglycerine_handle.id && material.flags.HasAny(Material.Flags.Explosive))
								{
									var amount_new = (requirement.amount * material.mass_per_unit) / material_nitroglycerine.mass_per_unit;

									requirement.amount = amount_new;
									requirement.material = material_nitroglycerine_handle;

									has_any = true;
								}
							}
						}

						if (has_any)
						{
							data.radius += MathF.Sqrt(data.radius * 1.50f);
							data.power += MathF.Sqrt(data.power * 2.50f);
							data.damage_terrain += MathF.Pow(data.damage_terrain * 3.50f, 0.75f);
							data.damage_entity += MathF.Pow(data.damage_entity * 2.50f, 0.75f);
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Explosive.Data>
			(
				identifier: "explosive.dud",
				category: "Explosive",
				name: "Dud Explosive",
				description: "Replaces all the explosive material with dirt.",

				apply_1: static (ref Augment.Context context, ref Explosive.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.damage_terrain = 0.00f;
					data.damage_entity = 0.00f;
					data.power = 1.00f;
					data.radius = 2.00f;
					data.pitch = 2.00f;
					data.volume = 0.60f;

					var amount_total = 0.00f;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.flags.HasAny(Material.Flags.Explosive))
							{
								amount_total += requirement.amount;
								requirement = default;
							}
						}
					}
					
					context.requirements_new.Add(Crafting.Requirement.Resource("soil", amount_total));				
				}
			));

			definitions.Add(Augment.Definition.New<Explosive.Data>
			(
				identifier: "explosive.essence",
				category: "Explosive",
				name: "Essence Payload",
				description: "Adds essence pellets to the explosive.",
				flags: Augment.Definition.Flags.Hidden,

				can_add: static (ref Augment.Context context, in Explosive.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle) && !context.HasComponent<EssenceContainer.Data>();
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Explosive.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();
					ref var h_essence = ref handle.GetData<IEssence.Handle>();

					var dirty = false;

					//dirty |= GUI.EnumInput("Essence", ref h_essence, size: new Vector2(GUI.GetRemainingWidth() * 0.60f, GUI.GetRemainingHeight()), show_label: false, close_on_select: true);
					//GUI.SameLine();
					dirty |= GUI.SliderIntLerp("Amount", ref modifier, 1, 20, size: GUI.GetRemainingSpace());


					return dirty;
				},
#endif

				apply_0: static (ref Augment.Context context, ref Explosive.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();
					ref var h_essence = ref handle.GetData<IEssence.Handle>();
					ref var essence_data = ref h_essence.GetData();
					if (essence_data.IsNotNull())
					{
						var amount = Maths.LerpInt(1, 20, modifier);

						ref var essence_container = ref context.GetOrAddComponent<EssenceContainer.Data>();
						if (essence_container.IsNotNull())
						{
							essence_container.h_essence = h_essence;
							essence_container.stability = 0.25f;
							essence_container.available = amount * Essence.essence_per_pellet;
						}

						context.requirements_new.Add(Crafting.Requirement.Resource(essence_data.h_material_pellet, amount));
					}
				}
			));

			definitions.Add(Augment.Definition.New<Explosive.Data>
			(
				identifier: "explosive.smoke",
				category: "Explosive",
				name: "Smoke Explosive",
				description: "Replaces all the explosive material with sulfur, which produces a lot of smoke.",

				apply_1: static (ref Augment.Context context, ref Explosive.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.damage_terrain = 0.00f;
					data.damage_entity = 0.00f;
					data.power = 1.00f;
					data.radius = 30.00f;
					data.pitch = 2.00f;
					data.volume = 0.01f;
					data.smoke_amount = 10.00f;
					data.sparks_amount = 0.10f;

					var amount_total = 0.00f;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.flags.HasAny(Material.Flags.Explosive))
							{
								amount_total += requirement.amount;
								requirement = default;
							}
						}
					}
					
					context.requirements_new.Add(Crafting.Requirement.Resource("sulfur", amount_total));				
				}
			));
		}
	}
}

