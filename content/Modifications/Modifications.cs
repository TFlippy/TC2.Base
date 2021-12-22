﻿using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		// TODO: Split this into multiple files
		private static void RegisterModifications(ref List<Modification.Definition> definitions)
		{
			definitions.Add(Modification.Definition.New<Health.Data>
			(
				identifier: "health.bomb_rigged",
				category: "Explosives",
				name: "Bomb-Rigged",
				description: "Item will explode after sustaining enough damage.",

				validate: static (ref Modification.Context context, in Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var pair = ref handle.GetData<(int amount, float threshold)>();
					pair.amount = Maths.Clamp(pair.amount, 1, 10);
					pair.threshold = Maths.Clamp(pair.threshold, 0.20f, 0.99f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var pair = ref handle.GetData<(int amount, float threshold)>();

					var size = GUI.GetRemainingSpace();

					var changed = false;
					changed |= GUI.SliderInt("##amount", ref pair.amount, 1, 10, "%d", size: new(size.X * 0.50f, size.Y));
					GUI.SameLine();
					changed |= GUI.SliderFloat("##threshold", ref pair.threshold, 0.20f, 0.99f, "%.2f", size: new(size.X * 0.50f, size.Y));

					return changed;
				},
#endif

				can_add: static (ref Modification.Context context, in Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return context.GetComponent<Explosive.Data>().IsNull();
				},

				apply_0: static (ref Modification.Context context, ref Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var pair = ref handle.GetData<(int amount, float threshold)>();

					ref var explosive = ref context.GetOrAddComponent<Explosive.Data>();
					explosive.power = 3.00f + (pair.amount * 0.80f);
					explosive.radius = 2.00f + (pair.amount * 0.50f);
					explosive.damage_entity = 1200.00f + (pair.amount * 400.00f);
					explosive.damage_terrain = 1800.00f + (pair.amount * 400.00f);
					explosive.health_threshold = pair.threshold;
					explosive.flags |= Explosive.Flags.Any_Damage | Explosive.Flags.Explode_When_Primed;
				},

				apply_1: static (ref Modification.Context context, ref Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var pair = ref handle.GetData<(int amount, float threshold)>();
					context.requirements_new.Add(Crafting.Requirement.Resource("nitroglycerine", pair.amount));
				}
			));

			definitions.Add(Modification.Definition.New<Health.Data>
			(
				identifier: "health.mushroom_glow",
				category: "Utility",
				name: "Mushroom Glow",
				description: "Glows in the dark.",

				can_add: static (ref Modification.Context context, in Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return context.GetComponent<Light.Data>().IsNull();
				},

				apply_0: static (ref Modification.Context context, ref Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var light = ref context.GetOrAddComponent<Light.Data>();
					light.color = new Vector4(0.600f, 1.000f, 0.400f, 1.250f);
					light.scale = new Vector2(32.000f, 32.000f);
					light.intensity = 1.000f;
					light.texture = "light_invsqr";
				},

				apply_1: static (ref Modification.Context context, ref Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("mushroom.green", 10.00f));
				}
			));

			definitions.Add(Modification.Definition.New<Health.Data>
			(
				identifier: "health.varnish",
				category: "Health",
				name: "Varnished Wood",
				description: "Applies varnish on item's wooden parts, improving their durability.",

				can_add: static (ref Modification.Context context, in Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return context.requirements_new.HasMaterial("wood") && !modifications.HasModification(handle);
				},

				apply_1: static (ref Modification.Context context, ref Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
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
					data.armor += 50.00f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref Material.GetMaterial("resin");
						body.mass_extra += total_amount * material.mass_per_unit * 0.30f;
					}
				}
			));

			definitions.Add(Modification.Definition.New<Health.Data>
			(
				identifier: "health.smirgl_structure",
				category: "Health",
				name: "Smirgl-Reinforced Structure",
				description: "Replaces entire structure with smirgl, greatly increasing durability.",

				can_add: static (ref Modification.Context context, in Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var has_ingot = false;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Ingot))
							{
								has_ingot = true;
								break;
							}	
						}
					}

					return has_ingot && !modifications.HasModification(handle);
				},

				apply_1: static (ref Modification.Context context, ref Health.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var ingot_amount = 0.00f;
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.50f;
							requirement.difficulty += 5.00f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetDefinition();
							if (material.flags.HasAll(Material.Flags.Ingot))
							{
								ingot_amount += requirement.amount;
								requirement = default;
							}
						}
					}

					var total_amount = 3.00f + (ingot_amount * 0.30f);
					context.requirements_new.Add(Crafting.Requirement.Resource("smirgl_ingot", total_amount));

					data.max += total_amount * 1000.00f;
					data.armor *= 1.35f;
					data.armor += 200.00f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref Material.GetMaterial("smirgl_ingot");
						body.mass_extra += total_amount * material.mass_per_unit * 0.70f;
					}
				}
			));

			//definitions.Add(Modification.Definition.New<Body.Data> // Can be used on any recipe which results in a prefab
			//(
			//	identifier: "body.efficient_crafting",
			//	name: "Efficient Crafting",
			//	description: "Rework the design to reduce material costs slightly.",

			//	apply_1: static (ref Modification.Context context, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
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

			definitions.Add(Modification.Definition.New<Fuse.Data>
			(
				identifier: "fuse.length",
				category: "Explosives",
				name: "Fuse Length",
				description: "Modifies fuse's length.",

				validate: static (ref Modification.Context context, in Fuse.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 0.50f, 10.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Fuse.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("##stuff", ref value, 0.50f, 10.00f, "%.2f");
				},
#endif

				apply_0: static (ref Modification.Context context, ref Fuse.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var value = ref handle.GetData<float>();
					data.time = value;
				}
			));

			definitions.Add(Modification.Definition.New<Fuse.Data>
			(
				identifier: "fuse.inextinguishable",
				category: "Explosives",
				name: "Inextinguishable Fuse",
				description: "Makes the fuse impossible to be extinguished.",

				apply_0: static (ref Modification.Context context, ref Fuse.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.failure_chance = 0.00f;
				}
			));

			definitions.Add(Modification.Definition.New<Overheat.Data>
			(
				identifier: "overheat.coolant",
				category: "Cooling",
				name: "Water-Cooled",
				description: "Increases cooling rate.",

				validate: static (ref Modification.Context context, in Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<int>();
					amount = Maths.Clamp(amount, 1, 40);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<int>();
					return GUI.SliderInt("##stuff", ref amount, 1, 40, "%d");
				},
#endif

				apply_0: static (ref Modification.Context context, ref Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<int>();
					data.cool_rate += amount * 4.00f;
				},

				apply_1: static (ref Modification.Context context, ref Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
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

			definitions.Add(Modification.Definition.New<Overheat.Data>
			(
				identifier: "overheat.movement_cooling",
				category: "Overheat",
				name: "Movement Cooling",
				description: "Increased cooling rate while in motion.",

				can_add: static (ref Modification.Context context, in Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return context.GetComponent<MovementCooling.Data>().IsNull();
				},

				apply_0: static (ref Modification.Context context, ref Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var cooling = ref context.GetOrAddComponent<MovementCooling.Data>();
					cooling.modifier = 0.10f;

					data.cool_rate *= 0.70f;
				}
			));

			definitions.Add(Modification.Definition.New<Overheat.Data>
			(
				identifier: "overheat.air_coolant",
				category: "Cooling",
				name: "Air-Cooled",
				description: "Increases cooling rate.",

				validate: static (ref Modification.Context context, in Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<int>();
					amount = Maths.Clamp(amount, 1, 10);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<int>();
					return GUI.SliderInt("##stuff", ref amount, 1, 10, "%d");
				},
#endif

				apply_0: static (ref Modification.Context context, ref Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<int>();
					data.cool_rate += amount * 9.00f;
				},

				apply_1: static (ref Modification.Context context, ref Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var amount = ref handle.GetData<int>();

					context.requirements_new.Add(Crafting.Requirement.Resource("iron_ingot", amount));

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref Material.GetMaterial("iron_ingot");
						body.mass_extra += amount * material.mass_per_unit;
					}
				}
			));

			definitions.Add(Modification.Definition.New<Overheat.Data>
			(
				identifier: "overheat.heat_resistant",
				category: "Cooling",
				name: "Heat-Resistant Components",
				description: "Dramatically increases maximum operating temperature at cost of extra weight and reduced cooling rate.",

				can_add: static (ref Modification.Context context, in Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.cool_rate *= 0.80f;
					data.heat_critical += 500.00f;
				},

				apply_1: static (ref Modification.Context context, ref Overheat.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
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

			definitions.Add(Modification.Definition.New<Body.Data>
			(
				identifier: "body.random_activation",
				category: "Utility",
				name: "Random Activation",
				description: "Item randomly activates on its own.",

				// This randomly causes a left click and a space bar (at the same time)
				// Working examples: guns, fuse explosives, drills, mounts (yes they will use whatever is on them), melee weapons, even medkits
				// Due to the wide variety of uses this has, this costs a large amount of materials
				// This doesn't aim, so using anything which uses aim direction requires additional setup

				apply_0: static (ref Modification.Context context, ref Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var random_activation = ref context.GetOrAddComponent<RandomActivation.Data>();
					random_activation.duration += 0.20f;
				},

				apply_1: static (ref Modification.Context context, ref Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("salt.motion", 10.00f)); // High cost

					if (!context.GetComponent<Melee.Data>().IsNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource("salt.motion", 10.00f)); // Even higher cost on melee weapons
					}
				}
			));

			definitions.Add(Modification.Definition.New<Body.Data>
			(
				identifier: "body.bulk",
				name: "Batch Production",
				description: "More efficient manufacturing process by producing multiple items in bulk.",

				validate: static (ref Modification.Context context, in Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var batch_size = ref handle.GetData<int>();
					batch_size = Maths.Clamp(batch_size, 1, 10);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Modification.Context context, in Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					ref var batch_size = ref handle.GetData<int>();
					return GUI.SliderInt("##stuff", ref batch_size, 1, 10, "%d");
				},
#endif

				can_add: static (ref Modification.Context context, in Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_1: static (ref Modification.Context context, ref Body.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
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

			definitions.Add(Modification.Definition.New<Cover.Data>
			(
				identifier: "cover.chitin_lined",
				category: "Cover",
				name: "Chitin-Lined",
				description: "Incorporate chitin lining into the armor, greatly improving its blocking capacity, while making it slightly more brittle.",

				can_add: static (ref Modification.Context context, in Cover.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Cover.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.threshold = Maths.Clamp(data.threshold - 0.25f, 0.00f, 1.00f);

					ref var health = ref context.GetComponent<Health.Data>();
					if (!health.IsNull())
					{
						health.max *= 0.95f;
					}
				},

				apply_1: static (ref Modification.Context context, ref Cover.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("chitin", 10.00f));
				}
			));
		}
	}
}

