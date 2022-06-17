using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
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

			//definitions.Add(Augment.Definition.New<Body.Data> // Can be used on any recipe which results in a prefab
			//(
			//	identifier: "body.efficient_crafting",
			//	name: "Efficient Crafting",
			//	description: "Rework the design to reduce material costs slightly.",

			//	apply_1: static (ref Augment.Context context, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//	{
			//		foreach (ref var requirement in context.requirements_new)
			//		{
			//			if (requirement.type == Crafting.Requirement.Type.Resource)
			//			{
			//				requirement.amount *= 0.80f;
			//			}
			//			else if (requirement.type == Crafting.Requirement.Type.Work)
			//			{
			//				requirement.amount *= 1.05f;
			//				requirement.difficulty += 2.50f;
			//			}
			//		}
			//	}
			//));

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

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.bulk",
				category: "Crafting",
				name: "Batch Production",
				description: "More efficient manufacturing process by producing multiple items in bulk.",

				validate: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var batch_size = ref handle.GetData<int>();
					batch_size = Maths.Clamp(batch_size, 1, 10);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var batch_size = ref handle.GetData<int>();
					return GUI.SliderInt("Count", ref batch_size, 1, 10);
				},
#endif

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref readonly var recipe_old = ref context.GetRecipeOld();
					ref var recipe_new = ref context.GetRecipeNew();

					ref var batch_size = ref handle.GetData<int>();

					recipe_new.min *= batch_size;
					recipe_new.max *= batch_size;
					recipe_new.step *= batch_size;

					for (int i = 0; i < context.requirements_new.Length; i++)
					{
						ref var requirement = ref context.requirements_new[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= MathF.Pow(0.99f, batch_size - 1);
							}
							else
							{
								requirement.amount *= MathF.Pow(0.98f, batch_size - 1);
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount *= MathF.Pow(0.95f, batch_size - 1);
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.amount *= MathF.Pow(0.93f, batch_size - 1);
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= MathF.Pow(0.90f, batch_size - 1);
								}
								break;

								default:
								{
									requirement.amount *= MathF.Pow(0.95f, batch_size - 1);
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.recycled",
				category: "Crafting",
				name: "Scrap-Recycled",
				description: "Lowers the production cost significantly, while also reducing overall item quality.",

				validate: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var ratio = ref handle.GetData<float>();
					ratio = Maths.Clamp(ratio, 0.10f, 0.65f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var ratio = ref handle.GetData<float>();
					return GUI.SliderFloat("Ratio", ref ratio, 0.10f, 0.65f);
				},
#endif

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					if (augments.HasAugment(handle)) return false;

					var has_valid_material = false;

					for (int i = 0; i < context.requirements_new.Length; i++)
					{
						ref var requirement = ref context.requirements_new[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.type == Material.Type.Metal || material.type == Material.Type.Wood || material.type == Material.Type.Fabric || material.type == Material.Type.Rubber)
							{
								has_valid_material = true;
								break;
							}
						}
					}

					return has_valid_material;
				},

				apply_0: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var ratio = ref handle.GetData<float>();

					ref var armor = ref context.GetComponent<Armor.Data>();
					if (!armor.IsNull())
					{
						armor.toughness *= 1.00f - ratio;
						armor.protection *= 1.00f - ratio;

						if (ratio > 0.15f)
						{
							armor.pain_modifier *= 1.00f + ratio;
						}
					}

					ref var health = ref context.GetComponent<Health.Data>();
					if (!health.IsNull())
					{
						health.max *= MathF.Pow(1.00f - ratio, 1.20f);
					}

					ref var gun = ref context.GetComponent<Gun.Data>();
					if (!gun.IsNull())
					{
						gun.stability *= MathF.Pow(1.00f - ratio, 1.30f);
						gun.failure_rate *= MathF.Pow(1.00f - ratio, 1.50f);

						if (ratio > 0.10f)
						{
							gun.failure_rate = MathF.Max(0.00f, gun.failure_rate + (ratio * 0.50f));
						}

						gun.reload_interval *= 1.00f + (ratio * 0.20f);
						gun.velocity_multiplier *= 1.00f - (ratio * 0.05f);
						gun.jitter_multiplier += ratio * 1.50f;
					}

					ref var melee = ref context.GetComponent<Melee.Data>();
					if (!melee.IsNull())
					{
						melee.damage_base *= 1.00f - ratio;
						melee.damage_bonus *= MathF.Pow(1.00f + ratio, 1.10f);
					}

					ref var holdable = ref context.GetComponent<Holdable.Data>();
					if (!holdable.IsNull())
					{
						holdable.force_multiplier *= MathF.Pow(1.00f - (ratio * 0.50f), 1.30f);
						holdable.torque_multiplier *= MathF.Pow(1.00f - (ratio * 0.10f), 1.20f);
					}

					ref var attachable = ref context.GetComponent<Attachable.Data>();
					if (!attachable.IsNull())
					{
						attachable.force_multiplier *= MathF.Pow(1.00f - (ratio * 0.40f), 1.20f);
						attachable.torque_multiplier *= MathF.Pow(1.00f - (ratio * 0.05f), 1.20f);
					}
				},

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var ratio = ref handle.GetData<float>();

					ref var material_scrap = ref Material.GetMaterial("scrap");
					var total_mass = 0.00f;

					for (int i = 0; i < context.requirements_new.Length; i++)
					{
						ref var requirement = ref context.requirements_new[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.type == Material.Type.Metal || material.type == Material.Type.Wood || material.type == Material.Type.Fabric || material.type == Material.Type.Rubber)
							{
								if (material.flags.HasAll(Material.Flags.Manufactured))
								{
									var removed_amount = requirement.amount * (ratio);
									requirement.amount -= removed_amount;

									total_mass += material.mass_per_unit * removed_amount * 2.10f;
								}
								else
								{
									var removed_amount = requirement.amount * (ratio * 0.70f);
									requirement.amount -= removed_amount;

									total_mass += material.mass_per_unit * removed_amount * 1.70f;
								}
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount *= MathF.Pow(1.00f - ratio, 1.50f);
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.amount *= MathF.Pow(1.00f - ratio, 1.30f);
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= MathF.Pow(1.00f - (ratio * 0.80f), 1.10f);
								}
								break;
							}
						}
					}

					context.requirements_new.Add(Crafting.Requirement.Resource(material_scrap.id, total_mass / material_scrap.mass_per_unit));
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
				identifier: "telescope.magnifying_lenses",
				category: "Telescope",
				name: "Magnifying Lenses",
				description: "Increases maximum range at cost of reduced field of view.",

				validate: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value = Maths.Clamp(value, 1.00f, 4.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Power", ref value, 1.00f, 4.00f);
				},
#endif

				can_add: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();

					data.max_distance *= value;
					data.zoom_min /= 0.50f + (value * 0.50f);
					data.zoom_max /= 0.50f + (value * 0.50f);
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
}

