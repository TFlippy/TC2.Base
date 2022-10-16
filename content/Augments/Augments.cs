using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class BaseMod
	{
		// TODO: Split this into multiple files
		private static void RegisterAugments(ref List<Augment.Definition> definitions)
		{
			definitions.Add(Augment.Definition.New<Health.Data>
			(
				identifier: "health.bomb_rigged",
				category: "Explosive",
				name: "Bomb-Rigged",
				description: "Item will explode after sustaining enough damage.",

				validate: static (ref Augment.Context context, in Health.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var pair = ref handle.GetData<(int amount, float threshold)>();
					pair.amount = Maths.Clamp(pair.amount, 10, 50);
					pair.threshold = Maths.Clamp(pair.threshold, 0.20f, 0.99f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Health.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var pair = ref handle.GetData<(int amount, float threshold)>();

					var size = GUI.GetRemainingSpace();

					var changed = false;
					changed |= GUI.SliderInt("Amount", ref pair.amount, 10, 50, size: new(size.X * 0.50f, size.Y));
					GUI.SameLine();
					changed |= GUI.SliderFloat("Threshold", ref pair.threshold, 0.20f, 0.99f, size: new(size.X * 0.50f, size.Y));

					return changed;
				},
#endif

				can_add: static (ref Augment.Context context, in Health.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !context.HasComponent<Explosive.Data>();
				},

				apply_0: static (ref Augment.Context context, ref Health.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var pair = ref handle.GetData<(int amount, float threshold)>();
					ref var material = ref Material.GetMaterial("nitroglycerine");

					ref var explosive = ref context.GetOrAddComponent<Explosive.Data>();
					explosive.power = 3.00f + (pair.amount * 0.80f * material.mass_per_unit);
					explosive.radius = 2.00f + (pair.amount * 0.50f * material.mass_per_unit);
					explosive.damage_entity = 1200.00f + (pair.amount * 400.00f * material.mass_per_unit);
					explosive.damage_terrain = 1800.00f + (pair.amount * 400.00f * material.mass_per_unit);
					explosive.health_threshold = pair.threshold;
					explosive.flags |= Explosive.Flags.Any_Damage | Explosive.Flags.Explode_When_Primed;
				},

				apply_1: static (ref Augment.Context context, ref Health.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var pair = ref handle.GetData<(int amount, float threshold)>();
					context.requirements_new.Add(Crafting.Requirement.Resource("nitroglycerine", pair.amount));
				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "health.mushroom_glow",
				category: "Utility",
				name: "Mushroom Glow",
				description: "Glows in the dark.",

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !context.HasComponent<Light.Data>() && !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var light = ref context.GetOrAddComponent<Light.Data>();
					light.color = new Vector4(0.600f, 1.000f, 0.400f, 1.250f);
					light.scale = new Vector2(32.000f, 32.000f);
					light.intensity = 1.000f;
					light.texture = "light_invsqr";
				},

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("mushroom.green", 10.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Health.Data>
			(
				identifier: "health.varnish",
				category: "Protection",
				name: "Varnished Wood",
				description: "Applies varnish on item's wooden parts, improving their durability.",

				can_add: static (ref Augment.Context context, in Health.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return context.requirements_new.HasMaterial("wood") && !augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Health.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					var amount = 0.00f;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.type == Material.Type.Wood)
							{
								amount += requirement.amount;
							}
						}
					}

					var total_amount = 3.00f + (amount * 0.10f);
					context.requirements_new.Add(Crafting.Requirement.Resource("resin", total_amount));

					data.max += total_amount * 85.00f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref Material.GetMaterial("resin");
						body.mass_extra += total_amount * material.mass_per_unit * 0.30f;
					}
				}
            ));


			definitions.Add(Augment.Definition.New<Fuse.Data>
			(
				identifier: "fuse.length",
				category: "Explosives",
				name: "Fuse Length",
				description: "Modifies fuse's length.",

				can_add: static (ref Augment.Context context, in Fuse.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				validate: static (ref Augment.Context context, in Fuse.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 0.50f, 10.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Fuse.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Timer", ref value, 0.50f, 10.00f);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Fuse.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					data.time = value;
				}
			));

			definitions.Add(Augment.Definition.New<Fuse.Data>
			(
				identifier: "fuse.inextinguishable",
				category: "Explosives",
				name: "Inextinguishable Fuse",
				description: "Makes the fuse impossible to be extinguished.",

				can_add: static (ref Augment.Context context, in Fuse.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Fuse.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.failure_chance = 0.00f;
				}
			));

			definitions.Add(Augment.Definition.New<Overheat.Data>
			(
				identifier: "overheat.coolant",
				category: "Heat Management",
				name: "Water-Cooled",
				description: "Increases cooling rate.",

				can_add: static (ref Augment.Context context, in Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				validate: static (ref Augment.Context context, in Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<int>();
					amount = Maths.Clamp(amount, 1, 40);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<int>();
					return GUI.SliderInt("Amount", ref amount, 1, 40);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<int>();
					data.cool_rate += amount * 4.00f;
				},

				apply_1: static (ref Augment.Context context, ref Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<int>();

					context.requirements_new.Add(Crafting.Requirement.Resource("water", amount));

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref Material.GetMaterial("water");
						body.mass_extra += amount * material.mass_per_unit;
					}
				}
			));

			definitions.Add(Augment.Definition.New<Overheat.Data>
			(
				identifier: "overheat.air_coolant",
				category: "Heat Management",
				name: "Air-Cooled",
				description: "Increases cooling rate while in motion.",

				can_add: static (ref Augment.Context context, in Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				validate: static (ref Augment.Context context, in Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<int>();
					amount = Maths.Clamp(amount, 1, 10);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<int>();
					return GUI.SliderInt("Amount", ref amount, 1, 10);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<int>();
				
					ref var cooling = ref context.GetOrAddComponent<MovementCooling.Data>();
					cooling.modifier = 1.00f + (0.50f * amount);
				},

				apply_1: static (ref Augment.Context context, ref Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<int>();

					context.requirements_new.Add(Crafting.Requirement.Resource("copper_plate", amount));

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref Material.GetMaterial("copper_plate");
						body.mass_extra += amount * material.mass_per_unit;
					}
				}
			));

			definitions.Add(Augment.Definition.New<Overheat.Data>
			(
				identifier: "overheat.heat_resistant",
				category: "Heat Management",
				name: "Heat-Resistant Components",
				description: "Dramatically increases maximum operating temperature at cost of extra weight and reduced cooling rate.",

				can_add: static (ref Augment.Context context, in Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.cool_rate *= 0.80f;
					data.heat_critical += 500.00f;
				},

				apply_1: static (ref Augment.Context context, ref Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					var amount = 0.00f;

					foreach (ref readonly var requirement in context.requirements_old)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAny(Material.Flags.Manufactured | Material.Flags.Metal))
							{
								amount += requirement.amount * 0.20f;
							}
						}
					}

					amount = MathF.Ceiling(amount);
					context.requirements_new.Add(Crafting.Requirement.Resource("smirgl_ingot", amount));

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref Material.GetMaterial("smirgl_ingot");
						body.mass_extra += amount * material.mass_per_unit;
					}
				}
			));

			definitions.Add(Augment.Definition.New<Control.Data>
			(
				identifier: "control.random_activation",
				category: "Utility",
				name: "Random Activation",
				description: "Item randomly activates on its own.",

				// This randomly causes a left click and a space bar (at the same time)
				// Working examples: guns, fuse explosives, drills, mounts (yes they will use whatever is on them), melee weapons, even medkits
				// Due to the wide variety of uses this has, this costs a large amount of materials
				// This doesn't aim, so using anything which uses aim direction requires additional setup

				can_add: static (ref Augment.Context context, in Control.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Control.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var random_activation = ref context.GetOrAddComponent<RandomActivation.Data>();
					random_activation.duration += 0.20f;
				},

				apply_1: static (ref Augment.Context context, ref Control.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("arcane_actuator", 1.00f)); // High cost

					if (!context.GetComponent<Melee.Data>().IsNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource("arcane_actuator", 1.00f)); // Even higher cost on melee weapons
					}
				}
			));

			//definitions.Add(Augment.Definition.New<Holdable.Data>
			//(
			//	identifier: "holdable.compact",
			//	name: "Compact",
			//	description: "Allows the item to be stored in toolbelt.",

			//	can_add: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//	{
			//		return !augments.HasAugment(handle) && !data.flags.HasAny(Holdable.Flags.Storable);
			//	},

			//	apply_0: static (ref Augment.Context context, ref Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//	{
			//		data.flags.SetFlag(Holdable.Flags.Storable, true);
			//	}
			//));

			definitions.Add(Augment.Definition.New<Telescope.Data>
			(
				identifier: "telescope.long_focus_lens",
				category: "Telescope",
				name: "Long-Focus Lens",
				description: "Increases maximum range at cost of reduced field of view.",

				validate: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value = Maths.Clamp(value, 1.00f, 2.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Power", ref value, 1.00f, 2.00f);
				},
#endif

				can_add: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return augments.GetCount(handle) <= 4;
				},

				apply_1: static (ref Augment.Context context, ref Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();

					data.zoom_modifier += data.zoom_modifier * 0.10f * value;
					data.max_distance *= value;
					data.zoom_min /= 0.50f + (value * 0.50f);
					data.zoom_max /= 0.50f + (value * 0.50f);

					context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 150, 10));
				}
			));

			definitions.Add(Augment.Definition.New<Telescope.Data>
			(
				identifier: "telescope.wide_angle_lens",
				category: "Telescope",
				name: "Wide-Angle Lens",
				description: "Increases field of view.",

				validate: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value = Maths.Clamp(value, 1.00f, 3.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Value", ref value, 1.00f, 3.00f);
				},
#endif

				can_add: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();

					data.zoom_modifier += data.zoom_modifier * 0.05f * value;
					data.max_distance /= 0.50f + (value * 0.50f);
					data.zoom_min += (value * 0.50f);
					data.zoom_max *= value;

					context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 250, 5));
				}
			));

			definitions.Add(Augment.Definition.New<Telescope.Data>
			(
				identifier: "telescope.precise_optics",
				category: "Telescope",
				name: "High-Precision Optics",
				description: "Reduces side-effects caused by high magnification.",

				validate: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value = Maths.Clamp(value, 0.00f, 1.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Value", ref value, 0.00f, 1.00f);
				},
#endif

				can_add: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return augments.GetCount(handle) <= 3;
				},

				apply_1: static (ref Augment.Context context, ref Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();

					data.zoom_modifier = Maths.Lerp(data.zoom_modifier, 0.50f, value * 0.90f);
					context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 120, (byte)MathF.Pow(7, 1.00f + (value * 1.50f))));
				}
			));

			definitions.Add(Augment.Definition.New<Telescope.Data>
			(
				identifier: "telescope.adjustable_optics",
				category: "Telescope",
				name: "Adjustable Optics",
				description: "Improves scope's adjustment speed.",

				validate: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value = Maths.Clamp(value, 0.00f, 1.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Value", ref value, 0.00f, 1.00f);
				},
#endif

				can_add: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return augments.GetCount(handle) <= 2;
				},

				apply_1: static (ref Augment.Context context, ref Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();

					data.zoom_modifier = MathF.Pow(data.zoom_modifier, 1.00f + (value * 0.02f));
					data.shake_modifier += data.shake_modifier * value * 0.40f;
					data.speed *= 1.00f + (value * 3.00f);
					data.zoom_min /= 1.00f + (value * 0.50f);
					data.zoom_max *= 1.00f + (value * 0.25f);

					context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 150, 7));
				}
			));

			definitions.Add(Augment.Definition.New<Equipment.Data>
			(
				identifier: "equipment.cursed",
				category: "Utility",
				name: "Cursed",
				description: "Covers the inner parts with tar, making it impossible to be unequipped.",

				can_add: static (ref Augment.Context context, in Equipment.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Equipment.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.flags.SetFlag(Equipment.Flags.Unremovable, true);
				},

				apply_1: static (ref Augment.Context context, ref Equipment.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("tar", 15.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Consumable.Data>
			(
				identifier: "consumable.alcohol",
				category: "Consumable",
				name: "Mix: Alcohol",
				description: "Mixes a dose of alcohol into the item.",

				can_add: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !context.HasComponent<Alcohol.Effect>();
				},

				validate: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value = Maths.Clamp(value, 1.00f, 500.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Amount", ref value, 1.00f, 500.00f, logarithmic: true);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();

					ref var component = ref context.GetOrAddComponent<Alcohol.Effect>();
					component.amount = amount;
				},

				apply_1: static (ref Augment.Context context, ref Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();

					ref var material = ref Material.GetMaterial("alcohol");
					context.requirements_new.Add(Crafting.Requirement.Resource(material.id, amount * 0.001f / material.mass_per_unit));
				}
			));

			definitions.Add(Augment.Definition.New<Consumable.Data>
			(
				identifier: "consumable.meth",
				category: "Consumable",
				name: "Mix: Methamphetamine",
				description: "Mixes a dose of methamphetamine into the item.",

				can_add: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !context.HasComponent<Meth.Effect>();
				},

				validate: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value = Maths.Clamp(value, 1.00f, 200.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Amount", ref value, 1.00f, 200.00f, logarithmic: true);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();

					ref var component = ref context.GetOrAddComponent<Meth.Effect>();
					component.amount = amount;
				},

				apply_1: static (ref Augment.Context context, ref Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();

					ref var material = ref Material.GetMaterial("meth");
					context.requirements_new.Add(Crafting.Requirement.Resource(material.id, amount * 0.001f / material.mass_per_unit));
				}
			));

			definitions.Add(Augment.Definition.New<Consumable.Data>
			(
				identifier: "consumable.morphine",
				category: "Consumable",
				name: "Mix: Morphine",
				description: "Mixes a dose of morphine into the item.",

				can_add: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !context.HasComponent<Morphine.Effect>();
				},

				validate: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value = Maths.Clamp(value, 1.00f, 100.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Amount", ref value, 1.00f, 100.00f, logarithmic: true);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();

					ref var component = ref context.GetOrAddComponent<Morphine.Effect>();
					component.amount = amount;
				},

				apply_1: static (ref Augment.Context context, ref Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();

					ref var material = ref Material.GetMaterial("morphine");
					context.requirements_new.Add(Crafting.Requirement.Resource(material.id, amount * 0.001f / material.mass_per_unit));
				}
			));

			definitions.Add(Augment.Definition.New<Consumable.Data>
			(
				identifier: "consumable.codeine",
				category: "Consumable",
				name: "Mix: Codeine",
				description: "Mixes a dose of codeine into the item.",

				can_add: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !context.HasComponent<Codeine.Effect>();
				},

				validate: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value = Maths.Clamp(value, 1.00f, 100.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Amount", ref value, 1.00f, 100.00f, logarithmic: true);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();

					ref var component = ref context.GetOrAddComponent<Codeine.Effect>();
					component.amount = amount;
				},

				apply_1: static (ref Augment.Context context, ref Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();

					ref var material = ref Material.GetMaterial("codeine");
					context.requirements_new.Add(Crafting.Requirement.Resource(material.id, amount * 0.001f / material.mass_per_unit));
				}
			));

			definitions.Add(Augment.Definition.New<Consumable.Data>
			(
				identifier: "consumable.paxilon",
				category: "Consumable",
				name: "Mix: Paxilon Hydrochlorate",
				description: "Mixes a dose of paxilon hydrochlorate into the item.",

				can_add: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !context.HasComponent<Paxilon.Effect>();
				},

				validate: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value = Maths.Clamp(value, 1.00f, 100.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Amount", ref value, 1.00f, 100.00f, logarithmic: true);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();

					ref var component = ref context.GetOrAddComponent<Paxilon.Effect>();
					component.amount = amount;
				},

				apply_1: static (ref Augment.Context context, ref Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();

					ref var material = ref Material.GetMaterial("paxilon");
					context.requirements_new.Add(Crafting.Requirement.Resource(material.id, amount * 0.001f / material.mass_per_unit));
				}
			));

			definitions.Add(Augment.Definition.New<Pill.Data>
			(
				identifier: "pill.extended_release",
				category: "Consumable",
				name: "Extended Release",
				description: "Lowers the release rate of a pill, spreading out the dose over a longer timespan.",

				validate: static (ref Augment.Context context, in Pill.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value = Maths.Clamp(value, 0.01f, 0.20f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Pill.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Multiplier", ref value, 0.01f, 0.20f);
				},
#endif

				can_add: static (ref Augment.Context context, in Pill.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Pill.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();

					ref var consumable = ref context.GetComponent<Consumable.Data>();
					if (!consumable.IsNull())
					{
						consumable.release_rate *= value;
					}
				}
			));
		}
	}
				},

				apply_1: static (ref Augment.Context context, ref Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					
}

