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

					var size = GUI.Rm;

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

					ref var material = ref IMaterial.Database.GetData("nitroglycerine");
					if (material.IsNotNull())
					{
						ref var explosive = ref context.GetOrAddComponent<Explosive.Data>();
						explosive.power = 3.00f + (pair.amount * 0.80f * material.mass_per_unit);
						explosive.radius = 2.00f + (pair.amount * 0.50f * material.mass_per_unit);
						explosive.damage_entity = 1200.00f + (pair.amount * 400.00f * material.mass_per_unit);
						explosive.damage_terrain = 1800.00f + (pair.amount * 400.00f * material.mass_per_unit);
						explosive.health_threshold = pair.threshold;
						explosive.flags |= Explosive.Flags.Any_Damage | Explosive.Flags.Explode_When_Primed;
					}
				},

				apply_1: static (ref Augment.Context context, ref Health.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var pair = ref handle.GetData<(int amount, float threshold)>();
					context.requirements_new.Add(Crafting.Requirement.Resource("nitroglycerine", pair.amount));
				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "utility.mushroom_glow",
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
					light.texture = "light.circle.00";
				},

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("mushroom.green", 3.00f));
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
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.type == Material.Type.Wood)
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
						ref var material = ref IMaterial.Database.GetData("resin");
						if (material.IsNotNull())
						{
							context.mass_new += total_amount * material.mass_per_unit * 0.30f;
						}
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

				//validate: static (ref Augment.Context context, in Fuse.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				//{
				//	ref var modifier = ref handle.GetData<float>();
				//	modifier = Maths.Clamp(modifier, 0.50f, 10.00f);

				//	return true;
				//},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Fuse.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();
					return GUI.SliderFloatLerp("Timer", ref modifier, 0.50f, 10.00f);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Fuse.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();
					data.time = Maths.Lerp(0.50f, 10.00f, modifier);
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
					context.requirements_new.Add(Crafting.Requirement.Resource("mushroom.green", 4.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Overheat.Data>
			(
				identifier: "overheat.coolant",
				category: "Heat Management",
				name: "Water-Cooled",
				description: "Increases heat capacity.",

				can_add: static (ref Augment.Context context, in Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();
					ref var offset = ref handle.GetData<Vector2>();

					var dirty = false;

					dirty |= GUI.SliderIntLerp("Amount", ref modifier, 1, 200, snap: 5, size: new Vector2(GUI.RmX * 0.50f, GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmX, GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					ref var modifier = ref handle.GetModifier();
					var amount = Maths.LerpInt(1, 200, modifier);

					var frame_index = 0u;

					if (amount <= 10) frame_index = 0;
					else if (amount <= 50) frame_index = 1;
					else if (amount <= 100) frame_index = 2;
					else frame_index = 3;

					var sprite = new Sprite(context.HasComponent<Gun.Data>() ? "augment.coolant.gun" : "augment.coolant", 24, 16, frame_index, 0);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), scale: new(offset.X > 0.00f ? 1.00f : -1.00f, offset.Y > 0.00f ? -1.00f : 1.00f), pivot: new(0.50f, 0.50f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();
					var amount = Maths.LerpInt(1, 200, modifier);

					data.heat_capacity_extra += amount * 2.00f;

					//data.cool_rate += (Maths.LerpInt(1, 200, modifier) * 10.00f) / (context.base_mass);

				},

				apply_1: static (ref Augment.Context context, ref Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();
					var amount = Maths.LerpInt(1, 200, modifier);

					context.requirements_new.Add(Crafting.Requirement.Resource("water", amount));

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref IMaterial.Database.GetData("water");
						if (material.IsNotNull())
						{
							context.mass_new += amount * material.mass_per_unit;
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Overheat.Data>
			(
				identifier: "overheat.radiator",
				category: "Heat Management",
				name: "Radiator",
				description: "Increases cooling rate.",

				can_add: static (ref Augment.Context context, in Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;

					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 5, size: new Vector2(GUI.RmX - (GUI.RmY * 3), GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_x", ref handle.flags, Augment.Handle.Flags.Mirror_X, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_y", ref handle.flags, Augment.Handle.Flags.Mirror_Y, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					//dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 5, size: new Vector2(GUI.RmX * 0.50f, GUI.RmY));
					//GUI.SameLine();
					//dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmX, GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 5, modifier);

					var sprite = new Sprite("augment.radiator", 24, 16, (uint)type, 0);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), scale: handle.GetScale(), pivot: new(0.50f, 0.50f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 5, modifier);
					var amount = 0.00f;

					switch (type)
					{
						case 0:
						{
							amount = 100.00f;
						}
						break;

						case 1:
						{
							amount = 500.00f;
						}
						break;

						case 2:
						{
							amount = 1000.00f;
						}
						break;

						case 3:
						{
							amount = 2500.00f;
						}
						break;

						case 4:
						{
							amount = 4500.00f;
						}
						break;

						case 5:
						{
							amount = 10000.00f;
						}
						break;
					}

					var size = Maths.Max(0.50f, data.offset.Length());
					var dist = Vector2.Distance(offset, data.offset);
					var dist_modifier = size / Maths.Max(size, dist);

					ref var cooling = ref context.GetOrAddComponent<MovementCooling.Data>();
					cooling.modifier = 1.00f + ((type + 1.00f) * 0.30f) * dist_modifier;

					data.cool_rate += (amount / context.mass_new) * dist_modifier;
				},

				apply_1: static (ref Augment.Context context, ref Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 5, modifier);

					var added_mass = 0.00f;

					switch (type)
					{
						case 0:
						{
							context.requirements_new.Add(Crafting.Requirement.Resource("steel.plate", 0.25f), ref added_mass);
						}
						break;

						case 1:
						{
							context.requirements_new.Add(Crafting.Requirement.Resource("steel.plate", 0.50f), ref added_mass);
						}
						break;

						case 2:
						{
							context.requirements_new.Add(Crafting.Requirement.Resource("steel.plate", 1.00f), ref added_mass);
						}
						break;

						case 3:
						{
							context.requirements_new.Add(Crafting.Requirement.Resource("steel.plate", 3.00f), ref added_mass);
						}
						break;

						case 4:
						{
							context.requirements_new.Add(Crafting.Requirement.Resource("steel.plate", 6.00f), ref added_mass);
						}
						break;

						case 5:
						{
							context.requirements_new.Add(Crafting.Requirement.Resource("steel.plate", 10.00f), ref added_mass);
						}
						break;
					}

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						context.mass_new += added_mass;
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
					data.temperature_critical += 500.00f;
				},

				apply_1: static (ref Augment.Context context, ref Overheat.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					var amount = 0.00f;

					foreach (ref readonly var requirement in context.requirements_old)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.flags.HasAny(Material.Flags.Manufactured | Material.Flags.Metal))
							{
								amount += requirement.amount * 0.20f;
							}
						}
					}

					amount = MathF.Ceiling(amount);
					context.requirements_new.Add(Crafting.Requirement.Resource("smirglum.ingot", amount));
					context.requirements_new.Add(Crafting.Requirement.Resource("mushroom.green", 20.00f));

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						ref var material = ref IMaterial.Database.GetData("smirglum.ingot");
						if (material.IsNotNull())
						{
							context.mass_new += amount * material.mass_per_unit;
						}
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
					context.requirements_new.Add(Crafting.Requirement.Resource("actuator", 1.00f)); // High cost

					if (!context.GetComponent<Melee.Data>().IsNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource("actuator", 1.00f)); // Even higher cost on melee weapons
					}
				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.bulk",
				category: "Crafting",
				name: "Bulk Production",
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

					for (var i = 0; i < context.requirements_new.Length; i++)
					{
						ref var requirement = ref context.requirements_new[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull())
							{
								if (material.flags.HasAll(Material.Flags.Manufactured))
								{
									requirement.amount *= MathF.Pow(0.99f, batch_size - 1);
								}
								else
								{
									requirement.amount *= MathF.Pow(0.98f, batch_size - 1);
								}
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work.GetIdentifier())
							{
								case "machining":
								{
									requirement.amount *= MathF.Pow(0.95f, batch_size - 1);
								}
								break;

								case "smithing":
								{
									requirement.amount *= MathF.Pow(0.93f, batch_size - 1);
								}
								break;

								case "assembling":
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
				flags: Augment.Definition.Flags.Hidden,

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
					return GUI.SliderFloat("Ratio", ref ratio, 0.10f, 0.65f, size: GUI.Rm);
				},
#endif

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					if (augments.HasAugment(handle)) return false;

					var has_valid_material = false;

					for (var i = 0; i < context.requirements_new.Length; i++)
					{
						ref var requirement = ref context.requirements_new[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && (material.type == Material.Type.Metal || material.type == Material.Type.Wood || material.type == Material.Type.Fabric || material.type == Material.Type.Rubber))
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
							gun.failure_rate = Maths.Max(0.00f, gun.failure_rate + (ratio * 0.50f));
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

					ref var attachable = ref context.GetComponent<Attachment.Data>();
					if (!attachable.IsNull())
					{
						attachable.force_multiplier *= MathF.Pow(1.00f - (ratio * 0.40f), 1.20f);
						attachable.torque_multiplier *= MathF.Pow(1.00f - (ratio * 0.05f), 1.20f);
					}
				},

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var ratio = ref handle.GetData<float>();

					ref var material_scrap = ref IMaterial.Database.GetData("scrap.ferrous", out var material_scrap_id);
					if (material_scrap.IsNotNull())
					{
						var total_mass = 0.00f;

						for (var i = 0; i < context.requirements_new.Length; i++)
						{
							ref var requirement = ref context.requirements_new[i];

							if (requirement.type == Crafting.Requirement.Type.Resource)
							{
								ref var material = ref requirement.material.GetData();
								if (material.IsNotNull() && (material.type == Material.Type.Metal || material.type == Material.Type.Wood || material.type == Material.Type.Fabric || material.type == Material.Type.Rubber))
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
								switch (requirement.work.GetIdentifier())
								{
									case "machining":
									{
										requirement.amount *= MathF.Pow(1.00f - ratio, 1.50f);
									}
									break;

									case "smithing":
									{
										requirement.amount *= MathF.Pow(1.00f - ratio, 1.30f);
									}
									break;

									case "assembling":
									{
										requirement.amount *= MathF.Pow(1.00f - (ratio * 0.80f), 1.10f);
									}
									break;
								}
							}
						}

						context.requirements_new.Add(Crafting.Requirement.Resource(material_scrap_id, total_mass / material_scrap.mass_per_unit));
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

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.lamp",
				category: "Utility",
				name: "Add: Lamp",
				description: "Attach a lamp.",

				validate: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					//offset.X.Clamp(-0.50f, 0.50f);
					//offset.Y.Clamp(-1.00f, 0.00f);
					offset.Snap(0.125f, out offset);

					return true;
				},

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle) && !context.HasComponent<Lamp.Data>();
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;

					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 7, size: new Vector2(GUI.RmX * 0.50f, GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_x", ref handle.flags, Augment.Handle.Flags.Mirror_X, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_y", ref handle.flags, Augment.Handle.Flags.Mirror_Y, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);
					//GUI.SameLine();
					//dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmX, GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					return dirty;

					//ref var offset = ref handle.GetData<Vector2>();

					//var size = GUI.Rm;
					//size.X *= 0.50f;

					//var dirty = false;
					////dirty |= GUI.Picker("offset", size: size, ref offset, min: new Vector2(-0.50f, -1.00f), max: new Vector2(0.50f, 0.00f));
					//dirty |= GUI.Picker("offset", size: new Vector2(GUI.RmX, GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					//return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 7, modifier);

					var sprite = new Sprite("augment.lamp", 16, 16, (byte)type, 0);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), scale: handle.GetScale(), pivot: new(0.50f, 0.50f));
					//draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), pivot: new(1.00f, 0.50f), scale: new(offset.X > 0.00f ? 1.00f : -1.00f, offset.Y > 0.00f ? -1.00f : 1.00f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 7, modifier);

					ref var lamp = ref context.GetOrAddComponent<Lamp.Data>();
					if (!lamp.IsNull())
					{
						lamp.flicker = 0.01f;

						ref var light = ref context.GetOrAddTrait<Lamp.Data, Light.Data>();
						if (light.IsNotNull())
						{
							var scale = new Vector2(handle.flags.HasAny(Augment.Handle.Flags.Mirror_X) ? -1.00f : 1.00f, handle.flags.HasAny(Augment.Handle.Flags.Mirror_Y) ? -1.00f : 1.00f);

							switch (type)
							{
								case 0:
								{
									light.color = new Vector4(1.00f, 0.75f, 0.10f, 1.45f);
									light.offset = offset + new Vector2(2.250f * scale.X, 0.000f);
									light.scale = new(16 * scale.X, 12);
									light.intensity = 1.00f;
									light.texture = Light.tex_light_cone_00;
								}
								break;

								case 1:
								{
									light.color = new Vector4(1.00f, 0.75f, 0.10f, 1.25f);
									light.offset = offset + new Vector2(-0.375f * scale.X, -0.125f * scale.Y);
									light.scale = new(20, 20);
									light.intensity = 1.00f;
									light.texture = Light.tex_light_circle_03;
								}
								break;

								case 2:
								{
									light.color = new Vector4(1.00f, 0.75f, 0.10f, 1.45f);
									light.offset = offset + new Vector2(-0.50f * scale.X, -0.35f * scale.Y);
									light.scale = new(24, 20);
									light.intensity = 1.00f;
									light.texture = Light.tex_light_circle_02;
								}
								break;

								case 3:
								{
									light.color = new Vector4(1.00f, 0.25f, 0.00f, 2.00f);
									light.offset = offset + new Vector2(-0.375f * scale.X, -0.050f * scale.Y);
									light.scale = new(16, 16);
									light.intensity = 1.00f;
									light.texture = Light.tex_light_circle_04;
								}
								break;

								case 4:
								{
									light.color = new Vector4(1.00f, 0.60f, 0.05f, 1.50f);
									light.offset = offset + new Vector2(-0.425f * scale.X, -0.25f * scale.Y);
									light.scale = new(24, 20);
									light.intensity = 1.00f;
									light.texture = Light.tex_light_circle_04;
								}
								break;

								case 5:
								{
									light.color = new Vector4(1.00f, 0.95f, 0.30f, 1.75f);
									light.offset = offset + new Vector2(7.00f * scale.X, 0.25f * scale.Y);
									light.scale = new(32 * scale.X, 24);
									light.intensity = 1.00f;
									light.texture = Light.tex_light_cone_00;
								}
								break;

								case 6:
								{
									light.color = new Vector4(1.00f, 0.75f, 0.10f, 1.50f);
									light.offset = offset + new Vector2(3.250f * scale.X, 0.000f);
									light.scale = new(13 * scale.X, 8);
									light.intensity = 1.000f;
									light.texture = Light.tex_light_cone_00;
								}
								break;

								case 7:
								{
									light.color = new Vector4(1.00f, 0.75f, 0.10f, 1.25f);
									light.offset = offset + new Vector2(4.00f * scale.X, 0.00f);
									light.scale = new(20 * scale.X, 16);
									light.intensity = 1.00f;
									light.texture = Light.tex_light_cone_00;
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Holdable.Data>
			(
				identifier: "holdable.scope",
				category: "Utility",
				name: "Add: Scope",
				description: "Attach a scope.",

				validate: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					offset.X.ClampRef(-0.50f, 0.50f);
					offset.Y.ClampRef(-1.00f, 0.00f);
					offset.Snap(0.125f, out offset);

					return true;
				},

				can_add: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle) && !context.HasComponent<Telescope.Data>() && data.flags.HasAll(Holdable.Flags.Storable); // && data.type != Gun.Type.Cannon && data.type != Gun.Type.AutoCannon && data.type != Gun.Type.Launcher;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					var size = GUI.Rm;
					size.X *= 0.50f;

					var dirty = false;
					dirty |= GUI.Picker("offset", "Offset", size: size, ref offset, min: new Vector2(-0.50f, -1.00f), max: new Vector2(0.50f, 0.00f));

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					draw.DrawSprite("augment.scope", Maths.Snap(offset, 0.125f), scale: new(1.00f, 1.00f), pivot: new(0.50f, 0.50f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					ref var scope = ref context.GetOrAddComponent<Telescope.Data>();
					if (!scope.IsNull())
					{
						scope.speed = 6.00f;
						scope.deadzone = 3.00f;
						scope.zoom_modifier = 0.50f;
						scope.zoom_min = 0.90f;
						scope.zoom_max = 0.90f;
						scope.min_distance = 12.00f;
						scope.max_distance = 80.00f;
					}
				}
			));

			definitions.Add(Augment.Definition.New<Telescope.Data>
			(
				identifier: "telescope.long_focus_lens",
				category: "Telescope",
				name: "Long-Focus Lens",
				description: "Increases maximum range at cost of reduced field of view.",

				validate: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value.ClampRef(1.00f, 2.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Power", ref value, 1.00f, 2.00f, size: GUI.Rm);
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

					context.requirements_new.Add(Crafting.Requirement.Work("assembling", 150, 10));
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
					value.ClampRef(1.00f, 3.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Value", ref value, 1.00f, 3.00f, size: GUI.Rm);
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

					context.requirements_new.Add(Crafting.Requirement.Work("assembling", 250, 5));
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
					value.ClampRef(0.00f, 1.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Value", ref value, 0.00f, 1.00f, size: GUI.Rm);
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
					context.requirements_new.Add(Crafting.Requirement.Work("assembling", 120, (byte)MathF.Pow(7, 1.00f + (value * 1.50f))));
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
					value.ClampRef(0.00f, 1.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Telescope.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Value", ref value, 0.00f, 1.00f, size: GUI.Rm);
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

					context.requirements_new.Add(Crafting.Requirement.Work("assembling", 150, 7));
				}
			));

			definitions.Add(Augment.Definition.New<Equipment.Data>
			(
				identifier: "equipment.cursed",
				category: "Equipment",
				name: "Cursed",
				description: "Covers the inner parts with tar, making it impossible to be unequipped.",

				can_add: static (ref Augment.Context context, in Equipment.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Equipment.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.flags.AddFlag(Equipment.Flags.Unremovable);
				},

				apply_1: static (ref Augment.Context context, ref Equipment.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("tar", 15.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Equipment.Data>
			(
				identifier: "equipment.cloth_padded",
				category: "Equipment",
				name: "Cloth Padding",
				description: "Pads the inner side of the armor with cloth.",

				can_add: static (ref Augment.Context context, in Equipment.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Equipment.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var armor = ref context.GetComponent<Armor.Data>();
					if (armor.IsNotNull())
					{
						armor.knockback_modifier *= 0.65f;
						armor.pain_modifier *= 0.80f;
					}
				},

				apply_1: static (ref Augment.Context context, ref Equipment.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("cloth", 5.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Equipment.Data>
			(
				identifier: "equipment.mushroom_stuffed",
				category: "Equipment",
				name: "Mushroom Stuffing",
				description: "Stuffs the inner side of the armor with soft mushroom bits.",

				can_add: static (ref Augment.Context context, in Equipment.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Equipment.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var armor = ref context.GetComponent<Armor.Data>();
					if (armor.IsNotNull())
					{
						armor.knockback_modifier = Maths.Max(armor.knockback_modifier - 0.30f, armor.knockback_modifier * 0.25f);
						armor.pain_modifier = Maths.Max((armor.pain_modifier * 0.90f) - 0.30f, armor.pain_modifier * 0.10f);
					}
				},

				apply_1: static (ref Augment.Context context, ref Equipment.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("mushroom", 20.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.ornament",
				category: "Misc",
				name: "Bling",
				description: "Improves bragging rights.",

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return augments.GetCount(handle) < 4;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;

					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 15, size: new Vector2(GUI.RmX * 0.60f, GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmX, GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var sprite = new Sprite("augment.ornament", 16, 16, (uint)type, 0);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), scale: new(offset.X > 0.00f ? -1.00f : 1.00f, offset.Y > 0.00f ? -1.00f : 1.00f), pivot: new(0.50f, 0.50f));
				},
#endif

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{

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
					value.ClampRef(1.00f, 500.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Amount", ref value, 1.00f, 500.00f, logarithmic: true, size: GUI.Rm);
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

					ref var material = ref IMaterial.Database.GetData("alcohol", out var material_id);
					if (material.IsNotNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource(material_id, amount * 0.001f / material.mass_per_unit));
					}
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
					value.ClampRef(1.00f, 200.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Amount", ref value, 1.00f, 200.00f, logarithmic: true, size: GUI.Rm);
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

					ref var material = ref IMaterial.Database.GetData("meth", out var material_id);
					if (material.IsNotNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource(material_id, amount * 0.001f / material.mass_per_unit));
					}
				}
			));

			definitions.Add(Augment.Definition.New<Consumable.Data>
			(
				identifier: "consumable.epinephrine",
				category: "Consumable",
				name: "Mix: Epinephrine",
				description: "Mixes a dose of epinephrine into the item.",

				can_add: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !context.HasComponent<Adrenaline.Effect>();
				},

				validate: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value.ClampRef(0.05f, 50.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Amount", ref value, 0.05f, 50.00f, logarithmic: true, size: GUI.Rm);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();

					ref var component = ref context.GetOrAddComponent<Adrenaline.Effect>();
					component.amount = amount;
				},

				apply_1: static (ref Augment.Context context, ref Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();

					//ref var material = ref IMaterial.Database.GetData("meth", out var material_id);
					//if (material.IsNotNull())
					//{
					//	context.requirements_new.Add(Crafting.Requirement.Resource(material_id, amount * 0.001f / material.mass_per_unit));
					//}
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
					value.ClampRef(1.00f, 100.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Amount", ref value, 1.00f, 100.00f, logarithmic: true, size: GUI.Rm);
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

					ref var material = ref IMaterial.Database.GetData("morphine", out var material_id);
					if (material.IsNotNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource(material_id, amount * 0.001f / material.mass_per_unit));
					}
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
					value.ClampRef(1.00f, 100.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Amount", ref value, 1.00f, 100.00f, logarithmic: true, size: GUI.Rm);
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

					ref var material = ref IMaterial.Database.GetData("codeine", out var material_id);
					if (material.IsNotNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource(material_id, amount * 0.001f / material.mass_per_unit));
					}
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
					value.ClampRef(1.00f, 100.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Consumable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Amount", ref value, 1.00f, 100.00f, logarithmic: true, size: GUI.Rm);
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

					ref var material = ref IMaterial.Database.GetData("paxilon", out var material_id);
					if (material.IsNotNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource(material_id, amount * 0.001f / material.mass_per_unit));
					}
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
					value.ClampRef(0.01f, 0.20f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Pill.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Multiplier", ref value, 0.01f, 0.20f, size: GUI.Rm);
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

			definitions.Add(Augment.Definition.New<Holdable.Data>
			(
				identifier: "holdable.grip.friction",
				category: "Utility",
				name: "High-Friction Grip",
				description: "Greatly increases grip strength. May lead to injury.",

				validate: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value.ClampRef(0.00f, 1.50f);

					return true;
				},

				can_add: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle) && !data.flags.HasAny(Holdable.Flags.Disable_Parent_Facing | Holdable.Flags.Disable_Pickup_Offset);
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Amount", ref value, 0.00f, 1.50f, size: GUI.Rm);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					value.ClampRef(0.00f, 1.50f);

					data.force_multiplier.MultCapped(Maths.Mulpo(value, 0.70f), 3.00f);
					data.torque_multiplier.MultCapped(Maths.Mulpo(value, 1.20f), 3.00f);
				},

				apply_1: static (ref Augment.Context context, ref Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("rubber", 1.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Holdable.Data>
			(
				identifier: "holdable.attachable",
				category: "Utility",
				name: "Attachment Connector",
				description: "Enables mounting this item to attachment slots.",

				validate: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					offset.X.ClampRef(-1.00f, 1.00f);
					offset.Y.ClampRef(-1.00f, 1.00f);
					offset.Snap(0.125f, out offset);

					return true;
				},

				can_add: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !context.HasComponent<Attachment.Data>();
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					var size = GUI.Rm;
					size.X *= 0.50f;

					var dirty = false;
					dirty |= GUI.Picker("offset", "Offset", size: size, ref offset, min: new Vector2(-1.00f, -1.00f), max: new Vector2(1.00f, 1.00f));

					//dirty |= GUI.SliderFloat("X", ref offset.X, -0.50f, 0.50f, size: size);
					//GUI.SameLine();
					//dirty |= GUI.SliderFloat("Y", ref offset.Y, -0.20f, 0.10f, size: size);

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					draw.DrawSprite("augment.attachable", Maths.Snap(offset, 0.125f), scale: new(1.00f, 1.00f), pivot: new(0.50f, 0.50f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					ref var attachable = ref context.GetOrAddComponent<Attachment.Data>();
					if (!attachable.IsNull())
					{
						attachable.offset = offset;
					}
				},

				apply_1: static (ref Augment.Context context, ref Holdable.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("machine_parts", 8.00f));
				}
			));

			definitions.Add(Augment.Definition.New<LandMine.Data>
			(
				identifier: "landmine.safety",
				category: "Utility",
				name: "Safety Markings",
				description: "Prevents faction members from stepping on the land mine.",

				can_add: static (ref Augment.Context context, in LandMine.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref LandMine.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.flags |= LandMine.Flags.Faction;
				},

				apply_1: static (ref Augment.Context context, ref LandMine.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("paper", 1));
				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.casing.steel",
				category: "Structure",
				name: "Steel Casing",
				description: "TODO: Desc",

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return augments.GetCount(handle) < 4;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;

					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 15, size: new Vector2(GUI.RmX - (GUI.RmY * 3), GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_x", ref handle.flags, Augment.Handle.Flags.Mirror_X, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_y", ref handle.flags, Augment.Handle.Flags.Mirror_Y, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var sprite = new Sprite("augment.casing.steel", 16, 16, (uint)type, 0);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), pivot: new(0.50f, 0.50f), scale: handle.GetScale());
				},
#endif

				apply_0: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var mass = 1.50f;
					var robustness = 1.00f;
					var size = 1.00f;
					var health_extra = 100.00f;
					var stability_base = 0.00f;
					var stability = 0.00f;

					switch (type)
					{
						case 0:
						{
							mass = 1.10f;
							robustness *= 1.50f;
							size *= 0.80f;
							health_extra = 335.00f;
							stability_base = 50.00f;
							stability = 195.00f;
						}
						break;

						case 1:
						{
							mass = 1.30f;
							robustness *= 1.60f;
							size *= 1.10f;
							health_extra = 225.00f;
							stability_base = 10.00f;
							stability = 180.00f;
						}
						break;

						case 2:
						{
							mass = 0.85f;
							robustness *= 1.30f;
							size *= 1.00f;
							health_extra = 125.00f;
							stability_base = 30.00f;
							stability = 150.00f;
						}
						break;

						case 3:
						{
							mass = 3.10f;
							robustness *= 2.50f;
							size *= 1.20f;
							health_extra = 635.00f;
							stability_base = 80.00f;
							stability = 350.00f;
						}
						break;

						case 4:
						{
							mass = 1.50f;
							robustness *= 1.40f;
							size *= 1.70f;
							health_extra = 325.00f;
							stability_base = 50.00f;
							stability = 160.00f;
						}
						break;

						case 5:
						{
							mass = 3.60f;
							robustness *= 3.30f;
							size *= 1.30f;
							health_extra = 835.00f;
							stability_base = 120.00f;
							stability = 250.00f;
						}
						break;

						case 6:
						{
							mass = 1.70f;
							robustness *= 1.40f;
							size *= 1.20f;
							health_extra = 135.00f;
							stability_base = 50.00f;
							stability = 245.00f;
						}
						break;

						case 7:
						{
							mass = 3.10f;
							robustness *= 2.50f;
							size *= 0.90f;
							health_extra = 135.00f;
							stability_base = 10.00f;
							stability = 395.00f;
						}
						break;

						case 8:
						{
							mass = 1.80f;
							robustness *= 1.50f;
							size *= 1.80f;
							health_extra = 535.00f;
							stability_base = 200.00f;
							stability = 135.00f;
						}
						break;
					}

					var mass_ratio = Maths.Normalize01(mass * 3.90f, context.mass_new);

					ref var gun = ref context.GetComponent<Gun.Data>();
					if (gun.IsNotNull())
					{
						var mult_receiver = 1.00f - Maths.Normalize01(Vector2.Distance(offset, gun.receiver_offset) - 0.25f * size, 0.75f * size).Pow2();
						var mult_barrel = (1.00f - Maths.Normalize01(MathF.Abs(offset.Y - gun.muzzle_offset.Y) - 0.50f * size, 0.60f)) * Maths.MidBias(gun.receiver_offset.X - 0.10f, (gun.receiver_offset.X + gun.muzzle_offset.X) * 0.50f, gun.muzzle_offset.X + 1.25f, offset.X);
						var mult_muzzle = 1.00f - Maths.Normalize01(Vector2.Distance(offset, new Vector2(Maths.Lerp(gun.muzzle_offset.X, gun.receiver_offset.X, 0.25f), gun.muzzle_offset.Y)) - 0.25f * size, 0.50f);

						gun.stability += stability_base + (stability * Maths.Lerp(mult_receiver, mult_barrel, 0.90f) * 0.50f * robustness * size);
						gun.failure_rate = Maths.Lerp(gun.failure_rate, Maths.Clamp01(gun.failure_rate * Maths.Mulpo(mult_receiver * -0.50f * robustness * size, mass_ratio * robustness)), 0.95f);
						//gun.recoil_multiplier *= Maths.Lerp(1.00f, Maths.Mulpo(mult_barrel, -0.12f), Maths.MidBias(0.00f, 0.25f, 0.625f, offset.Y - gun.muzzle_offset.Y) * mass_ratio);

						gun.reload_interval += size * 0.10f * mult_receiver;

						//ref var holdable = ref context.GetComponent<Holdable.Data>();
						//if (holdable.IsNotNull())
						//{
						//	if (offset.Y >= gun.muzzle_offset.Y) holdable.torque_multiplier *= Maths.Mulpo(mult_muzzle, 0.20f * mass_ratio * (grip - (bulkiness * size * 0.40f)));
						//	if (offset.X <= gun.receiver_offset.X + 0.25f) holdable.force_multiplier *= 1.00f + (Maths.MidBias(-0.25f, 0.125f, 0.625f, offset.Y - gun.receiver_offset.Y) * Maths.Mulpo(bulkiness, -0.50f) * mass_ratio * size * 0.40f);
						//}
					}

					ref var health = ref context.GetComponent<Health.Data>();
					if (health.IsNotNull())
					{
						health.max += health_extra;
						health.max *= Maths.Mulpo(0.15f, mass_ratio);
					}

					ref var armor = ref context.GetOrAddComponent<Armor.Data>();
					if (armor.IsNotNull())
					{
						armor.integrity_modifier += 0.10f * mass_ratio * size;
						armor.durability_modifier += 0.10f * mass_ratio * robustness;
						armor.toughness += 200.00f * mass_ratio;
						if (armor.material_type == Material.Type.None) armor.material_type = Material.Type.Metal;
						//armor.protection 
					}

					var h_material = new IMaterial.Handle("steel.plate");

					ref var material = ref h_material.GetData();
					if (material.IsNotNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource(h_material, mass / material.mass_per_unit));
					}

					context.mass_new += mass;
				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.casing.steel.large",
				category: "Structure",
				name: "Steel Casing (Large)",
				description: "TODO: Desc",

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return augments.GetCount(handle) < 8;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;

					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 15, size: new Vector2(GUI.RmX - (GUI.RmY * 3), GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_x", ref handle.flags, Augment.Handle.Flags.Mirror_X, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_y", ref handle.flags, Augment.Handle.Flags.Mirror_Y, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var sprite = new Sprite("augment.casing.steel.large", 24, 24, (uint)type, 0);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), pivot: new(0.50f, 0.50f), scale: handle.GetScale());
				},
#endif

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{

				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.casing.steel.vehicle",
				category: "Structure",
				name: "Steel Casing (Vehicle)",
				description: "TODO: Desc",

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return augments.GetCount(handle) < 8;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;

					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 15, size: new Vector2(GUI.RmX - (GUI.RmY * 3), GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_x", ref handle.flags, Augment.Handle.Flags.Mirror_X, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_y", ref handle.flags, Augment.Handle.Flags.Mirror_Y, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var sprite = new Sprite("augment.casing.steel.vehicle", 24, 24, (uint)type, 0);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), pivot: new(0.50f, 0.50f), scale: handle.GetScale());
				},
#endif

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{

				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.framework.steel",
				category: "Structure",
				name: "Steel Framework",
				description: "TODO: Desc",

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return augments.GetCount(handle) < 4;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;

					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 15, size: new Vector2(GUI.RmX - (GUI.RmY * 3), GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_x", ref handle.flags, Augment.Handle.Flags.Mirror_X, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_y", ref handle.flags, Augment.Handle.Flags.Mirror_Y, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var sprite = new Sprite("augment.framework.steel", 16, 16, (uint)type, 0);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), pivot: new(0.50f, 0.50f), scale: handle.GetScale());
				},
#endif

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var mass = 1.50f;
					var robustness = 1.00f;
					var size = 1.00f;
					var slant = 1.00f;
					var bulkiness = 1.00f;
					var grip = 1.00f;
					var health_extra = 100.00f;

					switch (type)
					{
						case 0:
						{
							mass = 0.90f;
							robustness *= 1.20f;
							size *= 0.70f;
							slant *= 1.35f;
							bulkiness *= 0.80f;
							grip *= 1.10f;
							health_extra = 75.00f;
						}
						break;

						case 1:
						{
							mass = 0.50f;
							robustness *= 0.80f;
							size *= 1.50f;
							slant *= 1.95f;
							bulkiness *= 1.50f;
							grip *= 0.80f;
							health_extra = 95.00f;
						}
						break;

						case 2:
						{
							mass = 1.00f;
							robustness *= 1.50f;
							size *= 1.10f;
							slant *= 0.20f;
							bulkiness *= 1.10f;
							grip *= 0.50f;
							health_extra = 125.00f;
						}
						break;

						case 3:
						{
							mass = 1.00f;
							robustness *= 0.90f;
							size *= 1.40f;
							slant *= 2.10f;
							bulkiness *= 1.10f;
							grip *= 2.20f;
							health_extra = 135.00f;
						}
						break;

						case 4:
						{
							mass = 1.40f;
							robustness *= 1.40f;
							size *= 1.10f;
							slant *= 0.70f;
							bulkiness *= 1.30f;
							grip *= 0.80f;
							health_extra = 145.00f;
						}
						break;

						case 5:
						{
							mass = 0.80f;
							robustness *= 1.10f;
							size *= 1.40f;
							slant *= 0.30f;
							bulkiness *= 0.90f;
							grip *= 5.00f;
							health_extra = 85.00f;
						}
						break;

						case 6:
						{
							mass = 1.90f;
							robustness *= 1.80f;
							size *= 1.40f;
							slant *= 0.20f;
							bulkiness *= 1.50f;
							grip *= 0.70f;
							health_extra = 345.00f;
						}
						break;

						case 7:
						{

						}
						break;

						case 8:
						{

						}
						break;
					}

					var mass_ratio = Maths.Normalize01(mass * 4.50f, context.mass_new);

					ref var gun = ref context.GetComponent<Gun.Data>();
					if (gun.IsNotNull())
					{
						var mult_receiver = 1.00f - Maths.Normalize01(Vector2.Distance(offset, gun.receiver_offset) - 0.375f * size, 0.75f * size);
						var mult_barrel = (1.00f - Maths.Normalize01(MathF.Abs(offset.Y - gun.muzzle_offset.Y) - 0.25f * size, 0.50f)) * (offset.X >= gun.receiver_offset.X ? 1.00f : 0.00f);
						var mult_muzzle = 1.00f - Maths.Normalize01(Vector2.Distance(offset, new Vector2(Maths.Lerp(gun.muzzle_offset.X, gun.receiver_offset.X, 0.25f), gun.muzzle_offset.Y)) - 0.25f * size, 0.50f);

						gun.stability += (health_extra * mult_barrel * 0.25f * mass_ratio * robustness * size);
						gun.failure_rate = Maths.Lerp(gun.failure_rate, Maths.Clamp01(gun.failure_rate * Maths.Mulpo((mult_barrel + mult_receiver) * -0.25f * robustness * bulkiness * size, mass_ratio)), 0.85f);
						gun.recoil_multiplier *= Maths.Lerp01(1.00f, Maths.Mulpo(mult_barrel, -0.12f), Maths.MidBias(0.00f, 0.25f, 0.625f, offset.Y - gun.muzzle_offset.Y) * mass_ratio);
						gun.jitter_multiplier *= Maths.Lerp01(1.00f, Maths.Mulpo(mult_muzzle, -0.12f), Maths.MidBias(0.00f, 0.25f, 0.625f, offset.Y - gun.muzzle_offset.Y) * mass_ratio);

						ref var holdable = ref context.GetComponent<Holdable.Data>();
						if (holdable.IsNotNull())
						{
							if (offset.Y >= gun.muzzle_offset.Y)
							{
								holdable.torque_multiplier *= Maths.Mulpo(mult_muzzle, 0.20f * mass_ratio * (grip - (bulkiness * size * 0.40f)));
								holdable.offset.X = Maths.Lerp(holdable.offset.X, offset.X, (grip * 0.20f * mass_ratio).Clamp01() * 0.25f);
							}
							if (offset.X <= gun.receiver_offset.X + 0.25f) holdable.force_multiplier *= 1.00f + (Maths.MidBias(-0.25f, 0.125f, 0.625f, offset.Y - gun.receiver_offset.Y) * Maths.Mulpo(bulkiness, -0.50f) * mass_ratio * size * 0.40f);
						}
					}

					ref var health = ref context.GetComponent<Health.Data>();
					if (health.IsNotNull())
					{
						health.max += health_extra;
						health.max *= Maths.Mulpo(0.15f, mass_ratio);
					}

					ref var armor = ref context.GetOrAddComponent<Armor.Data>();
					if (armor.IsNotNull())
					{
						armor.integrity_modifier += 0.10f * mass_ratio;
						armor.toughness += 120.00f * mass_ratio * size;
						if (armor.material_type == Material.Type.None) armor.material_type = Material.Type.Metal;
					}

					var h_material = new IMaterial.Handle("steel.ingot");

					ref var material = ref h_material.GetData();
					if (material.IsNotNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource(h_material, mass / material.mass_per_unit));
					}

					context.mass_new += mass;
				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.framework.steel.large",
				category: "Structure",
				name: "Steel Framework (Large)",
				description: "TODO: Desc",

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return augments.GetCount(handle) < 8;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;

					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 15, size: new Vector2(GUI.RmX - (GUI.RmY * 3), GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_x", ref handle.flags, Augment.Handle.Flags.Mirror_X, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_y", ref handle.flags, Augment.Handle.Flags.Mirror_Y, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var sprite = new Sprite("augment.framework.steel.large", 24, 24, (uint)type, 0);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), pivot: new(0.50f, 0.50f), scale: handle.GetScale());
				},
#endif

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{

				},

				finalize: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return true;
				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.casing.scrap",
				category: "Structure",
				name: "Scrap Casing",
				description: "TODO: Desc",

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return augments.GetCount(handle) < 8;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;

					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 15, size: new Vector2(GUI.RmX - (GUI.RmY * 3), GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_x", ref handle.flags, Augment.Handle.Flags.Mirror_X, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_y", ref handle.flags, Augment.Handle.Flags.Mirror_Y, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var sprite = new Sprite("augment.casing.scrap", 16, 16, (uint)type, 0);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), pivot: new(0.50f, 0.50f), scale: handle.GetScale());
				},
#endif

				apply_0: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var mass = 1.50f;
					var robustness = 1.00f;
					var size = 1.00f;
					var jank = 1.00f;
					var rust = 1.00f;

					switch (type)
					{
						case 0:
						{
							mass = 2.10f;
							robustness *= 1.80f;
							size *= 1.80f;
							jank *= 0.90f;
							rust *= 0.70f;
						}
						break;

						case 1:
						{
							mass = 0.70f;
							robustness *= 1.20f;
							size *= 0.80f;
							jank *= 1.10f;
							rust *= 0.50f;
						}
						break;

						case 2:
						{
							mass = 1.15f;
							robustness *= 0.850f;
							size *= 1.20f;
							jank *= 1.50f;
							rust *= 1.20f;
						}
						break;

						case 3:
						{
							mass = 4.10f;
							robustness *= 1.90f;
							size *= 1.10f;
							jank *= 0.20f;
							rust *= 1.20f;
						}
						break;

						case 4:
						{
							mass = 1.50f;
							robustness *= 1.10f;
							size *= 1.00f;
							jank *= 1.70f;
							rust *= 1.50f;
						}
						break;

						case 5:
						{
							mass = 5.60f;
							robustness *= 3.30f;
							size *= 1.90f;
							jank *= 0.30f;
							rust *= 1.20f;
						}
						break;

						case 6:
						{
							mass = 1.90f;
							robustness *= 1.10f;
							size *= 1.10f;
							jank *= 0.90f;
							rust *= 1.20f;
						}
						break;

						case 7:
						{
							mass = 1.40f;
							robustness *= 1.50f;
							size *= 1.10f;
							jank *= 1.20f;
							rust *= 1.50f;
						}
						break;

						case 8:
						{
							mass = 1.70f;
							robustness *= 1.50f;
							size *= 1.80f;
							jank *= 0.90f;
							rust *= 1.20f;
						}
						break;

						case 9:
						{
							mass = 1.70f;
							robustness *= 1.50f;
							size *= 1.30f;
							jank *= 1.70f;
							rust *= 1.20f;
						}
						break;

						case 10:
						{
							mass = 2.90f;
							robustness *= 1.50f;
							size *= 1.50f;
							jank *= 1.80f;
							rust *= 1.60f;
						}
						break;

						case 11:
						{
							mass = 1.90f;
							robustness *= 1.50f;
							size *= 1.50f;
							jank *= 1.00f;
							rust *= 1.40f;
						}
						break;

						case 12:
						{
							mass = 1.30f;
							robustness *= 1.20f;
							size *= 1.30f;
							jank *= 0.60f;
							rust *= 1.10f;
						}
						break;

						case 13:
						{
							mass = 1.50f;
							robustness *= 0.80f;
							size *= 1.90f;
							jank *= 1.20f;
							rust *= 1.50f;
						}
						break;

						case 14:
						{
							mass = 1.30f;
							robustness *= 1.50f;
							size *= 1.80f;
							jank *= 0.90f;
							rust *= 1.20f;
						}
						break;

						case 15:
						{
							mass = 1.30f;
							robustness *= 1.50f;
							size *= 1.80f;
							jank *= 0.90f;
							rust *= 1.20f;
						}
						break;
					}

					var mass_ratio = Maths.Normalize01(mass * 1.50f, context.mass_new);
					//var total_mass = 5.00f;
					//var mass_added = 0.00f;

					ref var material_scrap = ref IMaterial.Database.GetData("scrap.ferrous", out var material_scrap_id);
					if (material_scrap.IsNotNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource(material_scrap_id, mass / material_scrap.mass_per_unit));

						for (var i = 0; i < context.requirements_new.Length; i++)
						{
							ref var requirement = ref context.requirements_new[i];

							if (requirement.type == Crafting.Requirement.Type.Resource)
							{
								//ref var material = ref requirement.material.GetData();
								//if (material.IsNotNull() && (material.type == Material.Type.Metal || material.type == Material.Type.Wood || material.type == Material.Type.Fabric || material.type == Material.Type.Rubber))
								//{
								//	if (material.flags.HasAll(Material.Flags.Manufactured))
								//	{
								//		//requirement.AddMass()

								//		var removed_amount = requirement.amount * (0.80f);
								//		requirement.amount -= removed_amount;

								//		total_mass += material.mass_per_unit * removed_amount * 2.10f;
								//	}
								//	else
								//	{
								//		var removed_amount = requirement.amount * (0.70f);
								//		requirement.amount -= removed_amount;

								//		total_mass += material.mass_per_unit * removed_amount * 1.70f;
								//	}
								//}
							}
							else if (requirement.type == Crafting.Requirement.Type.Work)
							{
								switch (requirement.work.GetIdentifier())
								{
									case "machining":
									{
										requirement.amount *= 0.85f;
										requirement.difficulty = (byte)(requirement.difficulty * 0.75f);
									}
									break;

									case "smithing":
									{
										requirement.amount *= 0.77f;
										requirement.difficulty = (byte)Maths.Max(3, (requirement.difficulty * 0.90f) - 3.00f);
									}
									break;

									case "assembling":
									{
										requirement.amount *= 0.85f;
										requirement.difficulty = (byte)Maths.Max(3, (requirement.difficulty * 0.80f) - 2.00f);
									}
									break;
								}
							}
						}

						//context.requirements_new.Add(Crafting.Requirement.Resource(material_scrap_id, Maths.Max(total_mass, 5.00f) / material_scrap.mass_per_unit));
						//context.requirements_new.Add(Crafting.Requirement.Resource(material_scrap_id, Maths.Max(total_mass, 5.00f) / material_scrap.mass_per_unit));
					}

					var random = XorRandom.New(context.seed);

					var noise_a = Maths.Perlin(offset.X, offset.Y, random.NextFloatRange(0.50f, 1.50f), seed: (uint)context.seed);
					var noise_b = -Maths.Perlin(offset.X - (mass_ratio * 10.00f * jank), offset.Y + context.mass_old, noise_a * random.NextFloatRange(0.10f, 0.80f), seed: (uint)context.seed);
					var noise_c = Maths.Perlin(-offset.Y, offset.X, context.mass_new * mass_ratio * noise_a * random.NextFloatRange(0.50f, 1.50f), seed: (uint)context.seed);

					var augment_count = augments.GetCount(handle);

					ref var gun = ref context.GetComponent<Gun.Data>();
					if (gun.IsNotNull())
					{
						var mult_receiver = 1.00f - Maths.Normalize01(Vector2.Distance(offset, gun.receiver_offset) - 0.25f * size, 1.00f * size).Pow2();
						var mult_barrel = (1.00f - Maths.Normalize01(MathF.Abs(offset.Y - gun.muzzle_offset.Y) * size, 1.00f)) * Maths.MidBias(gun.receiver_offset.X - 0.25f, (gun.receiver_offset.X + gun.muzzle_offset.X) * 0.50f, gun.muzzle_offset.X + 1.25f, offset.X);
						var mult_muzzle = 1.00f - Maths.Normalize01(Vector2.Distance(offset, new Vector2(Maths.Lerp(gun.muzzle_offset.X, gun.receiver_offset.X, 0.25f), gun.muzzle_offset.Y)) + 0.25f * size, 1.50f);

						//gun.stability += stability_base + (stability * Maths.Lerp(mult_receiver, mult_barrel, 0.90f) * 0.50f * robustness * size);
						//gun.failure_rate = Maths.Lerp(gun.failure_rate, Maths.Clamp01(gun.failure_rate * Maths.Mulpo(mult_receiver * -0.50f * robustness * size, mass_ratio * robustness)), 0.95f);
						//gun.recoil_multiplier *= Maths.Lerp(1.00f, Maths.Mulpo(mult_barrel, -0.12f), Maths.MidBias(0.00f, 0.25f, 0.625f, offset.Y - gun.muzzle_offset.Y) * mass_ratio);

						gun.damage_multiplier = Maths.Lerp01(gun.damage_multiplier, gun.damage_multiplier * MathF.Abs((1.00f + (-noise_a * noise_b * noise_c * jank))), random.NextFloatRange(0.05f, 0.25f) * mass_ratio * mult_barrel * mult_receiver);
						gun.cycle_interval = MathF.Abs(Maths.Lerp01(gun.cycle_interval, gun.cycle_interval * Maths.Mulpo(noise_a, 1.25f), random.NextFloatRange(0.02f, 0.15f) * mult_receiver * mass_ratio));
						gun.jitter_multiplier += MathF.Abs(size * noise_a * jank);
						gun.stability = Maths.Lerp01(gun.stability, Maths.Lerp01(gun.stability, gun.stability - (mass * size * robustness * 250.00f * (1.45f + mult_receiver * robustness) * noise_b * noise_c), 0.15f * mass_ratio), 0.15f * mult_barrel * mass_ratio);
						gun.failure_rate = Maths.Lerp01(gun.failure_rate, gun.failure_rate + (rust * 0.09f * jank) + MathF.Abs(mass_ratio * 0.01f * mult_receiver * noise_a) * rust, MathF.Abs(noise_b * 0.80f) * jank * rust * (1.00f + (augment_count * augment_count * 1.50f)));
						gun.reload_interval = Maths.Lerp01(gun.reload_interval, Maths.Lerp01(gun.reload_interval, gun.reload_interval * size * 0.10f * mult_receiver * mass_ratio * noise_c * rust, 0.20f), mass_ratio * size);
						gun.velocity_multiplier = Maths.Lerp01(gun.velocity_multiplier, gun.velocity_multiplier * Maths.Mulpo(mult_muzzle * jank, -0.20f), 0.35f * mult_barrel * jank * rust);

						//var chance = size * robustness * gun.cycle_interval * mult_barrel * 0.10f;
						//App.WriteLine($"{chance}");

						//if (random.NextBool(jank * Maths.Clamp01(gun.failure_rate * 10.00f) * 0.05f * gun.jitter_multiplier * (1.00f + (augment_count * 0.80f))))
						//{
						//	if (context.mass_new >= 10.00f && random.NextBool(Maths.Lerp(0.50f, size * robustness * gun.cycle_interval * mult_barrel, 0.50f)))
						//	{
						//		gun.ammo_filter = Material.Flags.Ammo_TG;
						//		gun.max_ammo = (int)Maths.Clamp(jank * context.mass_new * robustness * 0.05f * MathF.Sqrt(gun.stability * 0.02f) * MathF.Abs(noise_c), 1, 4);
						//		gun.projectile_count = (int)gun.max_ammo;
						//		gun.ammo_per_shot = gun.projectile_count;
						//		gun.velocity_multiplier *= random.NextFloatRange(0.11f, 0.21f) / rust;
						//		gun.type = Gun.Type.Launcher;
						//		gun.sound_pitch *= 1.30f;
						//		gun.sound_shoot = "cannon_shoot";
						//		gun.sound_reload = "mrl_reload";
						//	}
						//	//else if (context.mass_new >= 25.00f && random.NextBool((Maths.Lerp(0.65f, 0.30f * gun.damage_multiplier * size * mult_muzzle * gun.sound_volume * gun.reload_interval * (1.00f + (augment_count * 0.30f)), 0.40f))))
						//	//{
						//	//	gun.ammo_filter = Material.Flags.Ammo_HW;
						//	//	gun.max_ammo = (int)Maths.Max(gun.max_ammo * 0.15f * robustness, 1);
						//	//	gun.projectile_count = (int)gun.max_ammo;
						//	//	gun.ammo_per_shot = gun.projectile_count;
						//	//	gun.type = Gun.Type.Cannon;
						//	//	gun.velocity_multiplier *= random.NextFloatRange(0.07f, 0.21f) / rust;
						//	//	gun.sound_shoot = "cannon_shoot";
						//	//	gun.sound_reload = "cannon_reload";
						//	//}
						//	else if (random.NextBool((Maths.Lerp(0.50f, gun.cycle_interval * jank * rust * mult_muzzle * (1.00f + (augment_count * 0.50f)), 0.50f))))
						//	{
						//		gun.ammo_filter = Material.Flags.Ammo_Musket;
						//		gun.flags |= Gun.Flags.Automatic;
						//	}
						//	else if (context.mass_new >= 8.00f && random.NextBool(robustness * jank * gun.recoil_multiplier * mass_ratio * mult_receiver * (1.00f + (augment_count * 0.50f))))
						//	{
						//		gun.ammo_filter = Material.Flags.Ammo_AC;
						//		gun.type = Gun.Type.Launcher;
						//	}
						//}

						//ref var holdable = ref context.GetComponent<Holdable.Data>();
						//if (holdable.IsNotNull())
						//{
						//	if (offset.Y >= gun.muzzle_offset.Y) holdable.torque_multiplier *= Maths.Mulpo(mult_muzzle, 0.20f * mass_ratio * (grip - (bulkiness * size * 0.40f)));
						//	if (offset.X <= gun.receiver_offset.X + 0.25f) holdable.force_multiplier *= 1.00f + (Maths.MidBias(-0.25f, 0.125f, 0.625f, offset.Y - gun.receiver_offset.Y) * Maths.Mulpo(bulkiness, -0.50f) * mass_ratio * size * 0.40f);
						//}
					}

					ref var health = ref context.GetComponent<Health.Data>();
					if (health.IsNotNull())
					{
						health.max *= 1.00f - (mass_ratio * 0.24f * jank);
					}

					ref var armor = ref context.GetOrAddComponent<Armor.Data>();
					if (armor.IsNotNull())
					{
						armor.toughness = Maths.MoveTowards(armor.toughness, 200.00f, 45.00f * jank) + (25.00f * robustness);
						armor.protection = Maths.MoveTowards(armor.protection, 0.50f, 0.05f * jank);
						armor.pain_modifier *= 1.15f * rust;
						armor.integrity_modifier *= 0.95f;
						if (armor.material_type == Material.Type.None) armor.material_type = Material.Type.Scrap;
					}

					//var h_material = new IMaterial.Handle("scrap.ferrous");

					//ref var material = ref h_material.GetData();
					//if (material.IsNotNull())
					//{
					//	context.requirements_new.Add(Crafting.Requirement.Resource(h_material, mass / material.mass_per_unit));
					//}

					context.mass_new += mass;
				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.casing.scrap.large",
				category: "Structure",
				name: "Scrap Casing (Large)",
				description: "TODO: Desc",

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return augments.GetCount(handle) < 8;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;

					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 15, size: new Vector2(GUI.RmX - (GUI.RmY * 3), GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_x", ref handle.flags, Augment.Handle.Flags.Mirror_X, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_y", ref handle.flags, Augment.Handle.Flags.Mirror_Y, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var sprite = new Sprite("augment.casing.scrap.large", 24, 24, (uint)type, 0);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), pivot: new(0.50f, 0.50f), scale: handle.GetScale());
				},
#endif

				apply_0: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var armor = ref context.GetOrAddComponent<Armor.Data>();
					if (!armor.IsNull())
					{
						armor.toughness = Maths.MoveTowards(armor.toughness, 200.00f, 45.00f) + 25.00f;
						armor.protection = Maths.MoveTowards(armor.protection, 0.50f, 0.05f);
						armor.pain_modifier *= 1.15f;
						armor.integrity_modifier *= 0.95f;
						if (armor.material_type == Material.Type.None) armor.material_type = Material.Type.Scrap;
					}

					ref var health = ref context.GetComponent<Health.Data>();
					if (!health.IsNull())
					{
						health.max *= 0.94f;
						health.max += 220.00f;
					}

					ref var gun = ref context.GetComponent<Gun.Data>();
					if (!gun.IsNull())
					{
						gun.stability *= 0.95f;
						gun.failure_rate += 0.02f;
						gun.reload_interval += 0.12f;
						gun.velocity_multiplier *= 0.97f;
						gun.jitter_multiplier += 0.10f;
					}

					ref var melee = ref context.GetComponent<Melee.Data>();
					if (!melee.IsNull())
					{
						melee.damage_base *= 0.98f;
						melee.damage_bonus += 25.00f;
						melee.cooldown += 0.10f;
					}

					ref var holdable = ref context.GetComponent<Holdable.Data>();
					if (!holdable.IsNull())
					{
						holdable.force_multiplier *= 1.10f;
						holdable.torque_multiplier += 0.10f;
						holdable.hints |= NPC.ItemHints.Junk;
					}

					ref var attachable = ref context.GetComponent<Attachment.Data>();
					if (!attachable.IsNull())
					{
						attachable.force_multiplier *= 0.90f;
						attachable.torque_multiplier *= 0.75f;
					}
				},

				apply_1: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var material_scrap = ref IMaterial.Database.GetData("scrap.ferrous", out var material_scrap_id);
					if (material_scrap.IsNotNull())
					{
						var total_mass = 5.00f;
						var mass_added = 0.00f;

						for (var i = 0; i < context.requirements_new.Length; i++)
						{
							ref var requirement = ref context.requirements_new[i];

							if (requirement.type == Crafting.Requirement.Type.Resource)
							{
								ref var material = ref requirement.material.GetData();
								if (material.IsNotNull() && (material.type == Material.Type.Metal || material.type == Material.Type.Wood || material.type == Material.Type.Fabric || material.type == Material.Type.Rubber))
								{
									if (material.flags.HasAll(Material.Flags.Manufactured))
									{
										//requirement.AddMass()

										var removed_amount = requirement.amount * (0.80f);
										requirement.amount -= removed_amount;

										total_mass += material.mass_per_unit * removed_amount * 2.10f;
									}
									else
									{
										var removed_amount = requirement.amount * (0.70f);
										requirement.amount -= removed_amount;

										total_mass += material.mass_per_unit * removed_amount * 1.70f;
									}
								}
							}
							else if (requirement.type == Crafting.Requirement.Type.Work)
							{
								switch (requirement.work.GetIdentifier())
								{
									case "machining":
									{
										requirement.amount *= 0.85f;
										requirement.difficulty = (byte)(requirement.difficulty * 0.75f);
									}
									break;

									case "smithing":
									{
										requirement.amount *= 0.77f;
										requirement.difficulty = (byte)Maths.Max(3, (requirement.difficulty * 0.90f) - 3.00f);
									}
									break;

									case "assembling":
									{
										requirement.amount *= 0.85f;
										requirement.difficulty = (byte)Maths.Max(3, (requirement.difficulty * 0.80f) - 2.00f);
									}
									break;
								}
							}
						}

						context.requirements_new.Add(Crafting.Requirement.Resource(material_scrap_id, Maths.Max(total_mass, 5.00f) / material_scrap.mass_per_unit));
					}
				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.casing.smirglum",
				category: "Structure",
				name: "Smirglum Casing",
				description: "TODO: Desc",

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return augments.GetCount(handle) < 4;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;

					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 15, size: new Vector2(GUI.RmX - (GUI.RmY * 3), GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_x", ref handle.flags, Augment.Handle.Flags.Mirror_X, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_y", ref handle.flags, Augment.Handle.Flags.Mirror_Y, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var sprite = new Sprite("augment.casing.smirglum", 16, 16, (uint)type, 0);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), pivot: new(0.50f, 0.50f), scale: handle.GetScale());
				},
#endif

				apply_0: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var mass = 1.50f;
					var robustness = 1.00f;
					var size = 1.00f;
					var health_extra = 100.00f;
					var stability_base = 0.00f;
					var stability = 0.00f;

					switch (type)
					{
						case 0:
						{
							mass = 4.50f;
							robustness *= 1.40f;
							size *= 0.80f;
							health_extra = 435.00f;
							stability_base = 150.00f;
							stability = 695.00f;
						}
						break;

						case 1:
						{
							mass = 9.00f;
							robustness *= 1.30f;
							size *= 1.20f;
							health_extra = 1225.00f;
							stability_base = 600.00f;
							stability = 750.00f;
						}
						break;

						case 2:
						{
							mass = 5.85f;
							robustness *= 1.20f;
							size *= 1.00f;
							health_extra = 725.00f;
							stability_base = 180.00f;
							stability = 550.00f;
						}
						break;

						case 3:
						{
							mass = 4.50f;
							robustness *= 1.50f;
							size *= 0.70f;
							health_extra = 335.00f;
							stability_base = 120.00f;
							stability = 450.00f;
						}
						break;

						case 4:
						{
							mass = 4.00f;
							robustness *= 1.50f;
							size *= 0.70f;
							health_extra = 255.00f;
							stability_base = 170.00f;
							stability = 400.00f;
						}
						break;

						case 5:
						{
							mass = 9.50f;
							robustness *= 2.00f;
							size *= 1.20f;
							health_extra = 935.00f;
							stability_base = 520.00f;
							stability = 950.00f;
						}
						break;

						case 6:
						{
							mass = 7.50f;
							robustness *= 1.50f;
							size *= 0.80f;
							health_extra = 435.00f;
							stability_base = 50.00f;
							stability = 945.00f;
						}
						break;

						case 7:
						{
							mass = 10.50f;
							robustness *= 1.30f;
							size *= 1.50f;
							health_extra = 735.00f;
							stability_base = 450.00f;
							stability = 295.00f;
						}
						break;

						case 8:
						{
							mass = 15.75f;
							robustness *= 1.80f;
							size *= 1.80f;
							health_extra = 1235.00f;
							stability_base = 500.00f;
							stability = 935.00f;
						}
						break;

						case 9:
						{
							mass = 9.20f;
							robustness *= 1.10f;
							size *= 2.10f;
							health_extra = 1835.00f;
							stability_base = 400.00f;
							stability = 835.00f;
						}
						break;

						case 10:
						{
							mass = 7.90f;
							robustness *= 1.00f;
							size *= 1.30f;
							health_extra = 535.00f;
							stability_base = 500.00f;
							stability = 735.00f;
						}
						break;

						case 11:
						{
							mass = 12.00f;
							robustness *= 1.50f;
							size *= 1.80f;
							health_extra = 535.00f;
							stability_base = 200.00f;
							stability = 135.00f;
						}
						break;

						case 12:
						{
							mass = 11.50f;
							robustness *= 1.80f;
							size *= 1.15f;
							health_extra = 1235.00f;
							stability_base = 500.00f;
							stability = 835.00f;
						}
						break;

						case 13:
						{
							mass = 15.75f;
							robustness *= 1.90f;
							size *= 1.20f;
							health_extra = 1435.00f;
							stability_base = 200.00f;
							stability = 1435.00f;
						}
						break;

						case 14:
						{
							mass = 21.75f;
							robustness *= 1.70f;
							size *= 2.00f;
							health_extra = 1735.00f;
							stability_base = 500.00f;
							stability = 2235.00f;
						}
						break;

						case 15:
						{
							mass = 13.75f;
							robustness *= 1.50f;
							size *= 1.80f;
							health_extra = 535.00f;
							stability_base = 200.00f;
							stability = 135.00f;
						}
						break;
					}

					var mass_ratio = Maths.Normalize01(mass * 3.50f, context.mass_new);

					ref var gun = ref context.GetComponent<Gun.Data>();
					if (gun.IsNotNull())
					{
						var mult_receiver = 1.00f - Maths.Normalize01(Vector2.Distance(offset, gun.receiver_offset) - 0.25f * size, 0.75f * size).Pow2();
						var mult_barrel = (1.00f - Maths.Normalize01(MathF.Abs(offset.Y - gun.muzzle_offset.Y) - 0.50f * size, 0.60f)) * Maths.MidBias(gun.receiver_offset.X - 0.10f, (gun.receiver_offset.X + gun.muzzle_offset.X) * 0.50f, gun.muzzle_offset.X + 1.25f, offset.X);
						var mult_muzzle = 1.00f - Maths.Normalize01(Vector2.Distance(offset, new Vector2(Maths.Lerp(gun.muzzle_offset.X, gun.receiver_offset.X, 0.25f), gun.muzzle_offset.Y)) - 0.25f * size, 0.50f);

						gun.stability += stability_base + (stability * Maths.Lerp(mult_receiver, mult_barrel, 0.90f) * 0.50f * robustness * size);
						gun.failure_rate = Maths.Lerp(gun.failure_rate, Maths.Clamp01(gun.failure_rate * Maths.Mulpo(mult_receiver * -0.50f * robustness * size, mass_ratio * robustness)), 0.95f);
						//gun.recoil_multiplier *= Maths.Lerp(1.00f, Maths.Mulpo(mult_barrel, -0.12f), Maths.MidBias(0.00f, 0.25f, 0.625f, offset.Y - gun.muzzle_offset.Y) * mass_ratio);

						gun.reload_interval += size * 0.10f * mult_receiver;

						//ref var holdable = ref context.GetComponent<Holdable.Data>();
						//if (holdable.IsNotNull())
						//{
						//	if (offset.Y >= gun.muzzle_offset.Y) holdable.torque_multiplier *= Maths.Mulpo(mult_muzzle, 0.20f * mass_ratio * (grip - (bulkiness * size * 0.40f)));
						//	if (offset.X <= gun.receiver_offset.X + 0.25f) holdable.force_multiplier *= 1.00f + (Maths.MidBias(-0.25f, 0.125f, 0.625f, offset.Y - gun.receiver_offset.Y) * Maths.Mulpo(bulkiness, -0.50f) * mass_ratio * size * 0.40f);
						//}
					}

					ref var health = ref context.GetComponent<Health.Data>();
					if (health.IsNotNull())
					{
						health.max += health_extra;
						health.max *= Maths.Mulpo(0.15f, mass_ratio);
					}

					ref var armor = ref context.GetOrAddComponent<Armor.Data>();
					if (armor.IsNotNull())
					{
						armor.integrity_modifier += 0.17f * mass_ratio * size;
						armor.durability_modifier += 0.12f * mass_ratio * robustness;
						armor.toughness = Maths.MoveTowards(armor.toughness, 1200.00f * robustness, stability_base * mass_ratio * size);
						if (armor.material_type == Material.Type.None) armor.material_type = Material.Type.Metal;
						//armor.protection 
					}

					var h_material = new IMaterial.Handle("smirglum.ingot");

					ref var material = ref h_material.GetData();
					if (material.IsNotNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource(h_material, mass / material.mass_per_unit));
					}
					context.requirements_new.Add(Crafting.Requirement.Resource("phlogiston", 1));

					context.mass_new += mass;
				}
			));

			definitions.Add(Augment.Definition.New<Body.Data>
			(
				identifier: "body.casing.smirglum.large",
				category: "Structure",
				name: "Smirglum Casing (Large)",
				description: "TODO: Desc",

				can_add: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return augments.GetCount(handle) < 8;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;

					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 15, size: new Vector2(GUI.RmX - (GUI.RmY * 3), GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_x", ref handle.flags, Augment.Handle.Flags.Mirror_X, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_y", ref handle.flags, Augment.Handle.Flags.Mirror_Y, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var sprite = new Sprite("augment.casing.smirglum.large", 24, 24, (uint)type, 0);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), pivot: new(0.50f, 0.50f), scale: handle.GetScale());
				},
#endif

				apply_0: static (ref Augment.Context context, ref Body.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var mass = 1.50f;
					var robustness = 1.00f;
					var size = 1.00f;
					var health_extra = 100.00f;
					var stability_base = 0.00f;
					var stability = 0.00f;

					switch (type)
					{
						case 0:
						{
							mass = 4.50f;
							robustness *= 1.40f;
							size *= 0.80f;
							health_extra = 435.00f;
							stability_base = 150.00f;
							stability = 695.00f;
						}
						break;

						case 1:
						{
							mass = 9.00f;
							robustness *= 1.30f;
							size *= 1.20f;
							health_extra = 1225.00f;
							stability_base = 600.00f;
							stability = 750.00f;
						}
						break;

						case 2:
						{
							mass = 45.00f;
							robustness *= 1.30f;
							size *= 1.40f;
							health_extra = 2725.00f;
							stability_base = 2250.00f;
							stability = 2550.00f;
						}
						break;

						case 3:
						{
							mass = 4.50f;
							robustness *= 1.50f;
							size *= 0.70f;
							health_extra = 335.00f;
							stability_base = 120.00f;
							stability = 450.00f;
						}
						break;

						case 4:
						{
							mass = 4.00f;
							robustness *= 1.50f;
							size *= 0.70f;
							health_extra = 255.00f;
							stability_base = 170.00f;
							stability = 400.00f;
						}
						break;

						case 5:
						{
							mass = 195.50f;
							robustness *= 2.50f;
							size *= 2.50f;
							health_extra = 7450.00f;
							stability_base = 2500.00f;
							stability = 4500.00f;
						}
						break;

						case 6:
						{
							mass = 7.50f;
							robustness *= 1.50f;
							size *= 0.80f;
							health_extra = 435.00f;
							stability_base = 50.00f;
							stability = 945.00f;
						}
						break;

						case 7:
						{
							mass = 10.50f;
							robustness *= 1.30f;
							size *= 1.50f;
							health_extra = 735.00f;
							stability_base = 450.00f;
							stability = 295.00f;
						}
						break;

						case 8:
						{
							mass = 15.75f;
							robustness *= 1.80f;
							size *= 1.80f;
							health_extra = 1235.00f;
							stability_base = 500.00f;
							stability = 935.00f;
						}
						break;

						case 9:
						{
							mass = 9.20f;
							robustness *= 1.10f;
							size *= 2.10f;
							health_extra = 1835.00f;
							stability_base = 400.00f;
							stability = 835.00f;
						}
						break;

						case 10:
						{
							mass = 7.90f;
							robustness *= 1.00f;
							size *= 1.30f;
							health_extra = 535.00f;
							stability_base = 500.00f;
							stability = 735.00f;
						}
						break;

						case 11:
						{
							mass = 12.00f;
							robustness *= 1.50f;
							size *= 1.80f;
							health_extra = 535.00f;
							stability_base = 200.00f;
							stability = 135.00f;
						}
						break;

						case 12:
						{
							mass = 11.50f;
							robustness *= 1.80f;
							size *= 1.15f;
							health_extra = 1235.00f;
							stability_base = 500.00f;
							stability = 835.00f;
						}
						break;

						case 13:
						{
							mass = 15.75f;
							robustness *= 1.90f;
							size *= 1.20f;
							health_extra = 1435.00f;
							stability_base = 200.00f;
							stability = 1435.00f;
						}
						break;

						case 14:
						{
							mass = 21.75f;
							robustness *= 1.70f;
							size *= 2.00f;
							health_extra = 1735.00f;
							stability_base = 500.00f;
							stability = 2235.00f;
						}
						break;

						case 15:
						{
							mass = 13.75f;
							robustness *= 1.50f;
							size *= 1.80f;
							health_extra = 535.00f;
							stability_base = 200.00f;
							stability = 135.00f;
						}
						break;
					}

					var mass_ratio = Maths.Normalize01(mass * 3.50f, context.mass_new);

					ref var gun = ref context.GetComponent<Gun.Data>();
					if (gun.IsNotNull())
					{
						var mult_receiver = 1.00f - Maths.Normalize01(Vector2.Distance(offset, gun.receiver_offset) - 0.25f * size, 0.75f * size).Pow2();
						var mult_barrel = (1.00f - Maths.Normalize01(MathF.Abs(offset.Y - gun.muzzle_offset.Y) - 0.50f * size, 0.60f)) * Maths.MidBias(gun.receiver_offset.X - 0.10f, (gun.receiver_offset.X + gun.muzzle_offset.X) * 0.50f, gun.muzzle_offset.X + 1.25f, offset.X);
						var mult_muzzle = 1.00f - Maths.Normalize01(Vector2.Distance(offset, new Vector2(Maths.Lerp(gun.muzzle_offset.X, gun.receiver_offset.X, 0.25f), gun.muzzle_offset.Y)) - 0.25f * size, 0.50f);

						gun.stability += stability_base + (stability * Maths.Lerp(mult_receiver, mult_barrel, 0.90f) * 0.50f * robustness * size);
						gun.failure_rate = Maths.Lerp(gun.failure_rate, Maths.Clamp01(gun.failure_rate * Maths.Mulpo(mult_receiver * -0.50f * robustness * size, mass_ratio * robustness)), 0.95f);
						//gun.recoil_multiplier *= Maths.Lerp(1.00f, Maths.Mulpo(mult_barrel, -0.12f), Maths.MidBias(0.00f, 0.25f, 0.625f, offset.Y - gun.muzzle_offset.Y) * mass_ratio);

						gun.reload_interval += size * 0.10f * mult_receiver;

						//ref var holdable = ref context.GetComponent<Holdable.Data>();
						//if (holdable.IsNotNull())
						//{
						//	if (offset.Y >= gun.muzzle_offset.Y) holdable.torque_multiplier *= Maths.Mulpo(mult_muzzle, 0.20f * mass_ratio * (grip - (bulkiness * size * 0.40f)));
						//	if (offset.X <= gun.receiver_offset.X + 0.25f) holdable.force_multiplier *= 1.00f + (Maths.MidBias(-0.25f, 0.125f, 0.625f, offset.Y - gun.receiver_offset.Y) * Maths.Mulpo(bulkiness, -0.50f) * mass_ratio * size * 0.40f);
						//}
					}

					ref var health = ref context.GetComponent<Health.Data>();
					if (health.IsNotNull())
					{
						health.max += health_extra;
						health.max *= Maths.Mulpo(0.15f, mass_ratio);
					}

					ref var armor = ref context.GetOrAddComponent<Armor.Data>();
					if (armor.IsNotNull())
					{
						armor.integrity_modifier += 0.17f * mass_ratio * size;
						armor.durability_modifier += 0.12f * mass_ratio * robustness;
						armor.toughness = Maths.MoveTowards(armor.toughness, 2500.00f * robustness, stability_base * mass_ratio * size);
						if (armor.material_type == Material.Type.None) armor.material_type = Material.Type.Metal;
						//armor.protection 
					}

					var h_material = new IMaterial.Handle("smirglum.ingot");

					ref var material = ref h_material.GetData();
					if (material.IsNotNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource(h_material, mass / material.mass_per_unit));
					}
					context.requirements_new.Add(Crafting.Requirement.Resource("phlogiston", 4));

					//App.WriteLine(mass);
					context.mass_new += mass;
				}
			));




			definitions.Add(Augment.Definition.New<SteamEngine.Data>
			(
				identifier: "steam_engine.failsafe",
				category: "Steam Engine",
				name: "Fail-Safe Mechanism [WIP]",
				description: "Greatly lowers chance of catastrophic failure at cost of reduced performance.",

				can_add: static (ref Augment.Context context, in SteamEngine.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in SteamEngine.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();

					var dirty = false;

					dirty |= GUI.SliderFloatLerp("Modifier", ref modifier, 0.10f, 1.00f, size: new Vector2(GUI.RmX, GUI.RmY));

					return dirty;
				},
#endif

				apply_1: static (ref Augment.Context context, ref SteamEngine.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();

					//data.burst_chance_modifier
				}
			));
		}
	}
}

