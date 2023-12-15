using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class BaseMod
	{
		private static void RegisterGunAugments(ref List<Augment.Definition> definitions)
		{
			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.expanded_magazine",
				category: "Gun (Ammo)",
				name: "Expanded Magazine",
				description: "Increases ammo capacity.",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 1.00f, 5.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();
					return GUI.SliderFloat("Value", ref amount, 1.00f, 5.00f, size: GUI.Rm);
				},
#endif

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					if (data.feed == Gun.Feed.Single || data.feed == Gun.Feed.Breech || data.feed == Gun.Feed.Front) return false;
					return !augments.HasAugment(handle);
				},

				finalize: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					var amount = handle.GetData<float>();
					var mass = 0.00f;

					switch (data.feed)
					{
						case Gun.Feed.Drum:
						{
							amount *= 3.00f;
							mass += amount * 0.60f;
						}
						break;

						case Gun.Feed.Cylinder:
						{
							amount += amount * 0.30f;
							mass += amount * 0.10f;
						}
						break;

						case Gun.Feed.Clip:
						{
							amount += amount * 3.00f;
							mass += amount * 0.20f;
						}
						break;

						case Gun.Feed.Magazine:
						{
							amount += amount * 4.00f;
							mass += amount * 0.30f;
						}
						break;
					}

					switch (data.type)
					{
						case Gun.Type.Cannon:
						{
							amount *= 2.00f;
							mass *= 3.00f;
						}
						break;

						case Gun.Type.AutoCannon:
						{
							amount *= 3.00f;
							mass *= 2.00f;
						}
						break;

						case Gun.Type.MachineGun:
						{
							amount *= 1.20f;
							mass *= 1.20f;
						}
						break;
					}

					if (data.ammo_filter.HasAll(Material.Flags.Ammo_Shell))
					{
						amount = MathF.Max(1.00f, MathF.Floor(amount * 0.20f));
						mass *= 4.00f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_AC))
					{
						amount = MathF.Max(1.00f, MathF.Floor(amount * 0.30f));
						mass *= 2.50f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_Rocket))
					{
						amount = MathF.Max(1.00f, MathF.Floor(amount * 0.40f));
						mass *= 1.20f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_MG))
					{
						amount = MathF.Max(1.00f, MathF.Floor(amount * 0.60f));
						mass *= 1.20f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_SG))
					{
						amount = MathF.Max(1.00f, MathF.Floor(amount * 0.85f));
						mass *= 1.10f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_HC))
					{
						amount = MathF.Max(1.00f, MathF.Floor(amount * 1.00f));
						mass *= 1.00f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_LC))
					{
						amount = MathF.Max(1.00f, MathF.Floor(amount * 1.50f));
						mass *= 0.90f;
					}

					data.max_ammo += amount;
					context.mass_new += mass * 0.50f;

					return true;
				},

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();
					foreach (ref readonly var requirement in context.requirements_old)
					{
						switch (requirement.type)
						{
							case Crafting.Requirement.Type.Resource:
							{
								ref var material = ref requirement.material.GetData();
								if (material.IsNotNull() && !material.flags.HasAll(Material.Flags.Manufactured) && material.type == Material.Type.Metal)
								{
									context.requirements_new.Add(Crafting.Requirement.Resource(requirement.material, requirement.amount * 0.40f * amount));
								}
							}
							break;

							case Crafting.Requirement.Type.Work:
							{
								switch (requirement.work)
								{
									case Work.Type.Smithing:
									{
										context.requirements_new.Add(Crafting.Requirement.Work(requirement.work, 50.00f * amount, (byte)MathF.Ceiling(5.00f * amount)));
									}
									break;
								}
							}
							break;
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.improved_rifling",
				category: "Gun (Barrel)",
				name: "Improved Rifling",
				description: "Improves accuracy and damage.",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 0.00f, 1.00f);

					return true;
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Value", ref value, 0.00f, 1.00f, size: GUI.Rm);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					var ratio = value;

					switch (data.type)
					{
						case Gun.Type.Shotgun:
						{
							data.velocity_multiplier *= Maths.Lerp(1.00f, 0.92f, ratio);
							data.damage_multiplier *= Maths.Lerp(1.00f, 0.85f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.60f, ratio);
						}
						break;

						case Gun.Type.Rifle:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.28f, ratio);
							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.13f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.10f, ratio);
							data.recoil_multiplier += Maths.Lerp(0.00f, 0.10f, ratio);
						}
						break;

						case Gun.Type.MachineGun:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.28f, ratio);
							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.13f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.10f, ratio);
							data.recoil_multiplier += Maths.Lerp(0.00f, 0.10f, ratio);
						}
						break;

						case Gun.Type.SMG:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.25f, ratio);
							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.12f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.15f, ratio);
							data.recoil_multiplier += Maths.Lerp(0.00f, 0.15f, ratio);
						}
						break;

						case Gun.Type.Handgun:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.20f, ratio);
							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.10f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.15f, ratio);
							data.recoil_multiplier += Maths.Lerp(0.00f, 0.05f, ratio);
						}
						break;

						default:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 1.25f, ratio);
							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.12f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.15f, ratio);
							data.recoil_multiplier += Maths.Lerp(0.00f, 0.10f, ratio);
						}
						break;
					}

					switch (data.action)
					{
						case Gun.Action.Blowback:
						{
							data.cycle_interval *= Maths.Lerp(1.00f, 1.19f, ratio);
						}
						break;

						case Gun.Action.Gas:
						{
							data.cycle_interval *= Maths.Lerp(1.00f, 1.11f, ratio);
							data.failure_rate += Maths.Lerp(0.00f, 0.003f, ratio);
							data.failure_rate *= Maths.Lerp(1.00f, 1.15f, ratio);
						}
						break;
					}
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					var ratio = value;

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.difficulty = (byte)(requirement.difficulty * Maths.Lerp(1.00f, 1.75f, ratio));
									requirement.amount += Maths.Lerp(0.00f, 300.00f, ratio);
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.difficulty = (byte)(requirement.difficulty * Maths.Lerp(1.00f, 1.50f, ratio));
									requirement.amount += Maths.Lerp(0.00f, 200.00f, ratio);
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.automatic",
				category: "Gun (Receiver)",
				name: "Mode: Automatic",
				description: "Converts fire mode to automatic.",

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return (data.action == Gun.Action.Gas || data.action == Gun.Action.Blowback || data.action == Gun.Action.Crank) && !data.flags.HasAny(Gun.Flags.Automatic) && !augments.HasAugment(handle);
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					if (!data.flags.HasAll(Gun.Flags.Automatic))
					{
						switch (data.action)
						{
							case Gun.Action.Gas:
							{
								data.flags |= Gun.Flags.Automatic;

								data.cycle_interval *= 1.10f;

								data.failure_rate *= 1.50f;
								data.failure_rate += 0.02f;
							}
							break;

							case Gun.Action.Blowback:
							case Gun.Action.Crank:
							{
								data.flags |= Gun.Flags.Automatic;

								data.cycle_interval *= 1.30f;

								data.failure_rate *= 2.50f;
								data.failure_rate += 0.05f;
							}
							break;
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.semi_automatic",
				category: "Gun (Receiver)",
				name: "Mode: Semi-Automatic",
				description: "Converts fire mode to semi-automatic.",

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return (data.action == Gun.Action.Gas || data.action == Gun.Action.Blowback || data.action == Gun.Action.Crank) && data.flags.HasAny(Gun.Flags.Automatic) && !augments.HasAugment(handle);
				},

				finalize: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					if (data.flags.HasAll(Gun.Flags.Automatic))
					{
						switch (data.action)
						{
							case Gun.Action.Gas:
							case Gun.Action.Blowback:
							case Gun.Action.Crank:
							{
								data.flags &= ~Gun.Flags.Automatic;
								data.failure_rate *= 0.10f;
								data.failure_rate -= MathF.Min(data.failure_rate, 0.02f);

								return true;
							}
							break;
						}
					}

					return false;
				}
			));

			//definitions.Add(Augment.Definition.New<Gun.Data>
			//(
			//	identifier: "gun.caliber_downgrade",
			//	name: "Caliber Downgrade",
			//	description: "Rechambers to a lower caliber.",

			//	can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//	{
			//		return !augments.HasAugment(handle) && data.ammo_filter.HasAny(Material.Flags.Ammo_AC | Material.Flags.Ammo_MG | Material.Flags.Ammo_HC);
			//	},

			//	apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//	{
			//		if (data.ammo_filter.HasAll(Material.Flags.Ammo_AC))
			//		{
			//			data.ammo_filter |= Material.Flags.Ammo_MG;
			//			data.ammo_filter &= ~Material.Flags.Ammo_AC;

			//			data.damage_multiplier *= 1.50f;
			//			data.velocity_multiplier *= 1.10f;
			//			data.failure_rate *= 0.20f;
			//			data.failure_rate += 0.10f;
			//			data.recoil_multiplier *= 0.50f;
			//			data.jitter_multiplier += 1.50f;
			//			data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.80f));

			//			data.stability = Maths.Clamp(data.stability * 3.50f, 0.00f, 1.00f);

			//			data.sound_pitch *= 1.15f;
			//			data.sound_volume *= 1.02f;

			//			data.smoke_size *= 0.90f;
			//			data.flash_size *= 0.90f;
			//		}
			//		else if (data.ammo_filter.HasAll(Material.Flags.Ammo_MG))
			//		{
			//			data.ammo_filter |= Material.Flags.Ammo_HC;
			//			data.ammo_filter &= ~Material.Flags.Ammo_MG;

			//			data.damage_multiplier *= 1.35f;
			//			data.velocity_multiplier *= 0.95f;
			//			data.failure_rate *= 1.90f;
			//			data.failure_rate += 0.50f;
			//			data.recoil_multiplier *= 0.70f;
			//			data.jitter_multiplier += 0.50f;
			//			data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.30f));

			//			data.stability = Maths.Clamp(data.stability * 1.20f, 0.00f, 1.00f);

			//			data.sound_pitch *= 1.15f;
			//			data.sound_volume *= 1.02f;

			//			data.smoke_size *= 0.90f;
			//			data.flash_size *= 0.90f;
			//		}
			//		else if (data.ammo_filter.HasAll(Material.Flags.Ammo_HC))
			//		{
			//			data.ammo_filter |= Material.Flags.Ammo_LC;
			//			data.ammo_filter &= ~Material.Flags.Ammo_HC;

			//			data.damage_multiplier *= 1.20f;
			//			data.velocity_multiplier *= 0.85f;
			//			data.failure_rate *= 1.70f;
			//			data.failure_rate += 0.50f;
			//			data.recoil_multiplier *= 0.60f;
			//			data.jitter_multiplier += 0.50f;
			//			data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.20f));

			//			data.stability = Maths.Clamp(data.stability * 1.40f, 0.00f, 1.00f);

			//			data.sound_pitch *= 1.10f;
			//			data.sound_volume *= 1.02f;

			//			data.smoke_size *= 0.90f;
			//			data.flash_size *= 0.90f;
			//		}
			//	}
			//));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.autocannon_caliber_downgrade",
				category: "Gun (Receiver)",
				name: "Autocannon: Caliber Downgrade",
				description: "Rechambers to a lower caliber, turning autocannon into oversized and sturdy machine gun.",

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					if (data.type != Gun.Type.AutoCannon) return false;
					return data.ammo_filter.HasAny(Material.Flags.Ammo_AC | Material.Flags.Ammo_MG | Material.Flags.Ammo_HC);
				},

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					if (data.ammo_filter.HasAll(Material.Flags.Ammo_AC))
					{
						data.ammo_filter |= Material.Flags.Ammo_MG;
						data.ammo_filter &= ~Material.Flags.Ammo_AC;

						data.damage_multiplier *= 0.60f;
						data.velocity_multiplier *= 0.80f;
						data.cycle_interval *= 0.45f;
						data.jitter_multiplier += 1.50f;
						data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.80f));

						//data.stability = Maths.Clamp(data.stability * 3.50f, 0.00f, 1.00f);

						data.sound_pitch *= 1.15f;
						data.sound_volume *= 1.02f;

						data.smoke_size *= 0.90f;
						data.flash_size *= 0.90f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_MG))
					{
						data.ammo_filter |= Material.Flags.Ammo_HC;
						data.ammo_filter &= ~Material.Flags.Ammo_MG;

						data.damage_multiplier *= 0.90f;
						data.velocity_multiplier *= 0.90f;
						data.cycle_interval *= 0.45f;
						data.jitter_multiplier += 0.50f;
						data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.30f));

						//data.stability = Maths.Clamp(data.stability * 1.20f, 0.00f, 1.00f);

						data.sound_pitch *= 1.15f;
						data.sound_volume *= 1.02f;

						data.smoke_size *= 0.90f;
						data.flash_size *= 0.90f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_HC))
					{
						data.ammo_filter |= Material.Flags.Ammo_LC;
						data.ammo_filter &= ~Material.Flags.Ammo_HC;

						data.damage_multiplier *= 0.90f;
						data.velocity_multiplier *= 0.45f;
						data.cycle_interval *= 0.90f;
						data.jitter_multiplier += 0.50f;
						data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.20f));

						//data.stability = Maths.Clamp(data.stability * 1.40f, 0.00f, 1.00f);

						data.sound_pitch *= 1.10f;
						data.sound_volume *= 1.02f;

						data.smoke_size *= 0.90f;
						data.flash_size *= 0.90f;
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.rapid_fire_mechanism",
				category: "Gun (Receiver)",
				name: "Rapid-Fire Mechanism",
				description: "Greatly increases fire rate, at the cost of worsened ballistics.",

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return (data.action == Gun.Action.Gas || data.action == Gun.Action.Blowback) && !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.cycle_interval *= 0.50f;
					data.damage_multiplier *= 0.72f;
					data.velocity_multiplier *= 0.85f;

					data.stability *= 0.90f - MathF.Min(data.failure_rate * 0.30f, 0.30f);
					data.stability = Maths.Lerp(data.stability, MathF.Max(data.stability - ((75.00f + (context.mass_new * 5.00f)) / Maths.Clamp(data.cycle_interval * 1.50f, 0.10f, 1.50f)), data.stability * 0.25f), 0.50f);
				},

				finalize: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.failure_rate *= 0.60f;
					data.jitter_multiplier += 2.50f;

					//ref var body = ref context.GetComponent<Body.Data>();
					//if (!body.IsNull())
					//{
					//	body.mass_multiplier *= 0.90f;
					//}

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 1.20f;
									requirement.difficulty = (byte)(requirement.difficulty * 1.20f);
								}
								break;

								case Work.Type.Woodworking:
								{
									requirement.amount *= 1.20f;
									requirement.difficulty = (byte)(requirement.difficulty * 1.20f);
								}
								break;

								case Work.Type.Machining:
								{
									requirement.amount *= 1.20f;
									requirement.difficulty = (byte)(requirement.difficulty * 1.20f);
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 1.50f;
									requirement.difficulty = (byte)(requirement.difficulty * 1.20f);
								}
								break;
							}
						}
					}

					return true;
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= 1.50f;
							}
						}
					}

					context.requirements_new.Add(Crafting.Requirement.Resource("lubricant", 5.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.improved_ammo_loading",
				category: "Gun (Receiver)",
				name: "Improved Ammo Loading",
				description: "Improves reliability and slightly increases rate of fire, while also increasing manufacturing costs.",

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return (data.action == Gun.Action.Gas || data.action == Gun.Action.Blowback) && !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.cycle_interval *= 0.70f;
				},

				finalize: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.failure_rate *= 0.60f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 0.90f;
					}

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 1.20f;
									requirement.difficulty = (byte)(requirement.difficulty * 1.20f);
								}
								break;

								case Work.Type.Woodworking:
								{
									requirement.amount *= 1.20f;
									requirement.difficulty = (byte)(requirement.difficulty * 1.20f);
								}
								break;

								case Work.Type.Machining:
								{
									requirement.amount *= 1.20f;
									requirement.difficulty = (byte)(requirement.difficulty * 1.20f);
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 1.50f;
									requirement.difficulty = (byte)(requirement.difficulty * 1.20f);
								}
								break;
							}
						}
					}

					return true;
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= 1.50f;
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.stacked_charge",
				category: "Gun (Receiver)",
				name: "Stacked Charge",
				description: "Allows to load more than one round per barrel, at the cost of increased complexity and manufacturing costs. Note that it doesn't improve cooling, so use at your own risk.",

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return (data.feed == Gun.Feed.Funnel || data.feed == Gun.Feed.Front || data.feed == Gun.Feed.Breech || data.feed == Gun.Feed.Single) && !augments.HasAugment(handle);
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();
					return GUI.SliderIntLerp("Count", ref modifier, 1, 3);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();
					var count = Maths.LerpInt(1, 3, modifier);

					data.failure_rate += 0.05f * data.barrel_count * count;
					data.failure_rate *= 1.20f;
					data.stability *= Maths.Mulpo(data.failure_rate, -0.10f * data.barrel_count); // MathF.Min(data.failure_rate * data.barrel_count, data.stability);
					data.jitter_multiplier *= MathF.Pow(1.50f, count);
				},

				finalize: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();
					var count = Maths.LerpInt(1, 3, modifier);

					data.max_ammo += data.barrel_count * count;

					//ref var body = ref context.GetComponent<Body.Data>();
					//if (!body.IsNull())
					//{
					//	body.mass_multiplier *= 1.10f;
					//}

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 1.20f;
									requirement.difficulty = (byte)(requirement.difficulty * 1.20f);
								}
								break;

								case Work.Type.Woodworking:
								{
									requirement.amount *= 1.20f;
									requirement.difficulty = (byte)(requirement.difficulty * 1.20f);
								}
								break;

								case Work.Type.Machining:
								{
									requirement.amount *= 1.20f;
									requirement.difficulty = (byte)(requirement.difficulty * 1.20f);
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 1.10f;
									requirement.difficulty = (byte)(requirement.difficulty * 1.20f);
								}
								break;
							}
						}
					}

					return true;
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= 1.10f;
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.caliber_conversion",
				category: "Gun (Receiver)",
				name: "Caliber Conversion",
				description: "Rechambers to a different caliber.",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<int>();
					value = Maths.Clamp(value, -2, 2); // TODO: Make this clamped between available calibers

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<int>();
					return GUI.SliderInt("Caliber", ref value, -2, 2);
				},
#endif

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					if (data.type == Gun.Type.AutoCannon) return false;
					return !augments.HasAugment(handle) && data.ammo_filter.HasAny(Material.Flags.Ammo_LC | Material.Flags.Ammo_HC | Material.Flags.Ammo_MG | Material.Flags.Ammo_AC);
				},

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<int>();

					var count = Math.Abs(value);
					if (value > 0)
					{
						for (var i = 0; i < count; i++)
						{
							if (data.ammo_filter.HasAll(Material.Flags.Ammo_LC))
							{
								data.ammo_filter |= Material.Flags.Ammo_HC;
								data.ammo_filter &= ~Material.Flags.Ammo_LC;

								data.damage_multiplier *= 0.90f;
								data.velocity_multiplier *= 0.90f;
								data.failure_rate *= 1.60f;
								data.failure_rate += 0.40f;
								//data.recoil_multiplier *= 1.70f;
								data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 0.60f));

								//data.stability -= MathF.Min(data.stability, 0.30f);
								//data.stability = Maths.Clamp(data.stability * 0.80f, 0.00f, 1.00f);

								data.sound_pitch *= 0.82f;
								data.sound_volume *= 1.20f;

								data.smoke_size *= 1.30f;
								data.flash_size *= 1.10f;
							}
							else if (data.ammo_filter.HasAll(Material.Flags.Ammo_HC))
							{
								data.ammo_filter |= Material.Flags.Ammo_MG;
								data.ammo_filter &= ~Material.Flags.Ammo_HC;

								data.damage_multiplier *= 0.85f;
								data.velocity_multiplier *= 0.85f;
								data.failure_rate *= 1.40f;
								data.failure_rate += 0.40f;
								//data.recoil_multiplier *= 1.80f;
								data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 0.50f));

								//data.stability -= MathF.Min(data.stability, 0.40f);
								//data.stability = Maths.Clamp(data.stability * 0.80f, 0.00f, 1.00f);

								data.sound_pitch *= 0.87f;
								data.sound_volume *= 1.25f;

								data.smoke_size *= 1.30f;
								data.flash_size *= 1.10f;
							}
							else if (data.ammo_filter.HasAll(Material.Flags.Ammo_MG))
							{
								data.ammo_filter |= Material.Flags.Ammo_AC;
								data.ammo_filter &= ~Material.Flags.Ammo_MG;

								data.damage_multiplier *= 0.70f;
								data.velocity_multiplier *= 0.50f;
								data.failure_rate *= 2.20f;
								data.failure_rate += 0.40f;
								//data.recoil_multiplier *= 3.50f;
								data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 0.10f));

								//data.stability -= MathF.Min(data.stability, 0.80f);
								//data.stability = Maths.Clamp(data.stability * 0.70f, 0.00f, 1.00f);

								data.sound_pitch *= 0.67f;
								data.sound_volume *= 1.25f;

								data.smoke_size *= 1.50f;
								data.flash_size *= 1.10f;
							}
						}
					}
					else if (value < 0)
					{
						for (var i = 0; i < count; i++)
						{
							if (data.ammo_filter.HasAll(Material.Flags.Ammo_AC))
							{
								data.ammo_filter |= Material.Flags.Ammo_MG;
								data.ammo_filter &= ~Material.Flags.Ammo_AC;

								data.damage_multiplier *= 1.50f;
								data.velocity_multiplier *= 1.10f;
								data.failure_rate *= 0.20f;
								data.failure_rate += 0.10f;
								//data.recoil_multiplier *= 0.50f;
								data.jitter_multiplier += 1.50f;
								data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.80f));

								//data.stability = Maths.Clamp(data.stability * 3.50f, 0.00f, 1.00f);

								data.sound_pitch *= 1.15f;
								data.sound_volume *= 1.02f;

								data.smoke_size *= 0.90f;
								data.flash_size *= 0.90f;
							}
							else if (data.ammo_filter.HasAll(Material.Flags.Ammo_MG))
							{
								data.ammo_filter |= Material.Flags.Ammo_HC;
								data.ammo_filter &= ~Material.Flags.Ammo_MG;

								data.damage_multiplier *= 1.05f;
								data.velocity_multiplier *= 0.95f;
								data.failure_rate *= 1.90f;
								data.failure_rate += 0.50f;
								//data.recoil_multiplier *= 0.70f;
								data.jitter_multiplier += 0.50f;
								data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.30f));

								//data.stability = Maths.Clamp(data.stability * 1.20f, 0.00f, 1.00f);

								data.sound_pitch *= 1.15f;
								data.sound_volume *= 1.02f;

								data.smoke_size *= 0.90f;
								data.flash_size *= 0.90f;
							}
							else if (data.ammo_filter.HasAll(Material.Flags.Ammo_HC))
							{
								data.ammo_filter |= Material.Flags.Ammo_LC;
								data.ammo_filter &= ~Material.Flags.Ammo_HC;

								data.damage_multiplier *= 1.02f;
								data.velocity_multiplier *= 0.85f;
								data.failure_rate *= 1.70f;
								data.failure_rate += 0.50f;
								//data.recoil_multiplier *= 0.60f;
								data.jitter_multiplier += 0.50f;
								data.max_ammo = MathF.Max(1.00f, MathF.Floor(data.max_ammo * 1.20f));

								//data.stability = Maths.Clamp(data.stability * 1.40f, 0.00f, 1.00f);

								data.sound_pitch *= 1.10f;
								data.sound_volume *= 1.02f;

								data.smoke_size *= 0.90f;
								data.flash_size *= 0.90f;
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.recoil_reduction",
				category: "Gun (Frame)",
				name: "Recoil Reduction",
				description: "Reduces recoil.",
				flags: Augment.Definition.Flags.Hidden,

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 0.00f, 1.00f);

					return true;
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Value", ref value, 0.00f, 1.00f, size: GUI.Rm);
				},
#endif

				// TODO: lerp the values based on ratio
				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					var ratio = value;

					switch (data.type)
					{
						case Gun.Type.Handgun:
						{
							data.damage_multiplier *= Maths.Lerp(1.00f, 0.97f, ratio);
							data.recoil_multiplier = Maths.Lerp(data.recoil_multiplier, MathF.Max(0.20f, data.recoil_multiplier - 0.12f), ratio);
						}
						break;

						case Gun.Type.Rifle:
						case Gun.Type.Shotgun:
						case Gun.Type.SMG:
						case Gun.Type.MachineGun:
						case Gun.Type.AutoCannon:
						{
							data.recoil_multiplier = MathF.Max(0.10f, data.recoil_multiplier - 0.07f);
							data.recoil_multiplier *= 0.85f;
							data.velocity_multiplier *= 0.91f;
							data.damage_multiplier *= 0.94f;
							//data.stability = Maths.Clamp(data.stability * 1.09f, 0.00f, 1.00f);

							switch (data.action)
							{
								case Gun.Action.Blowback:
								{
									data.cycle_interval *= 1.17f;
								}
								break;

								case Gun.Action.Gas:
								{
									data.cycle_interval *= 1.11f;
									data.failure_rate += 0.01f;
									data.failure_rate *= 1.10f;
								}
								break;
							}
						}
						break;

						default:
						{
							data.recoil_multiplier = MathF.Max(0.10f, data.recoil_multiplier - 0.07f);
							data.recoil_multiplier *= 0.85f;
							data.velocity_multiplier *= 0.87f;
							data.damage_multiplier *= 0.95f;
							//data.stability = Maths.Clamp(data.stability * 1.07f, 0.00f, 1.00f);

							switch (data.action)
							{
								case Gun.Action.Blowback:
								{
									data.cycle_interval *= 1.17f;
								}
								break;

								case Gun.Action.Gas:
								{
									data.cycle_interval *= 1.11f;
									data.failure_rate += 0.01f;
									data.failure_rate *= 1.10f;
								}
								break;
							}
						}
						break;
					}
				}
			));

			//			definitions.Add(Augment.Definition.New<Gun.Data>
			//			(
			//				identifier: "gun.barrel_extension",
			//				category: "Gun (Barrel)",
			//				name: "Barrel Extension",
			//				description: "Increases muzzle velocity and damage.",

			//				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//				{
			//					ref var amount = ref handle.GetData<float>();
			//					amount = Maths.Clamp(amount, 1.00f, 2.00f);

			//					return true;
			//				},

			//				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//				{
			//					return !augments.HasAugment(handle);
			//				},

			//#if CLIENT
			//				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//				{
			//					ref var value = ref handle.GetData<float>();
			//					return GUI.SliderFloat("Value", ref value, 1.00f, 2.00f);
			//				},
			//#endif

			//				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//				{
			//					ref var value = ref handle.GetData<float>();
			//					var ratio = value - 1.00f;

			//					switch (data.type)
			//					{
			//						case Gun.Type.Handgun:
			//						{
			//							//var mult = Maths.Clamp(data.velocity_multiplier / (300.00f * (value * value)), 0.01f, 2.00f);
			//							var mult = 1.00f - ratio;

			//							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.12f * mult, ratio);
			//							data.damage_multiplier *= Maths.Lerp(1.00f, 1.25f * mult, ratio);
			//							data.recoil_multiplier *= Maths.Lerp(1.00f, 1.25f, value);
			//							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.20f, ratio);
			//						}
			//						break;

			//						case Gun.Type.Rifle:
			//						{
			//							//var mult = Maths.Clamp(data.velocity_multiplier / (800.00f * (value)), 0.01f, 2.00f);
			//							var mult = 1.00f - ratio;

			//							data.velocity_multiplier *= Maths.Lerp(1.00f, 1.13f * mult, ratio);
			//							data.damage_multiplier *= Maths.Lerp(1.00f, 1.27f * mult, ratio);
			//							data.recoil_multiplier *= Maths.Lerp(1.00f, 1.13f * value, ratio);
			//							data.jitter_multiplier *= Maths.Lerp(1.00f, 0.30f * mult, ratio);
			//						}
			//						break;

			//						case Gun.Type.Shotgun:
			//						{
			//							data.damage_multiplier *= 1.21f;
			//							data.recoil_multiplier *= 1.10f;
			//							data.jitter_multiplier *= 0.40f;
			//							data.velocity_multiplier *= 1.21f;
			//						}
			//						break;

			//						case Gun.Type.SMG:
			//						{
			//							data.damage_multiplier *= 1.15f;
			//							data.recoil_multiplier *= 1.15f;
			//							data.jitter_multiplier *= 0.70f;
			//							data.velocity_multiplier *= 1.07f;
			//						}
			//						break;

			//						default:
			//						{
			//							data.damage_multiplier *= 1.15f;
			//							data.recoil_multiplier *= 1.15f;
			//							data.jitter_multiplier *= 0.70f;
			//							data.velocity_multiplier *= 1.07f;
			//						}
			//						break;
			//					}

			//					switch (data.action)
			//					{
			//						case Gun.Action.Blowback:
			//						{
			//							data.cycle_interval *= 1.20f;
			//						}
			//						break;

			//						case Gun.Action.Gas:
			//						{
			//							data.cycle_interval *= 1.14f;
			//							data.failure_rate += 0.01f;
			//							data.failure_rate *= 1.10f;
			//						}
			//						break;
			//					}
			//				}
			//			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.balanced_receiver",
				category: "Gun (Receiver)",
				name: "Balanced Receiver",
				description: "Stabilizes the receiver, increasing reliability.",

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					switch (data.type)
					{
						case Gun.Type.Handgun:
						{
							data.damage_multiplier *= 0.98f;
							data.velocity_multiplier *= 0.97f;
							data.failure_rate *= 0.18f;
							data.cycle_interval *= 0.98f;
							data.stability *= 1.05f;
						}
						break;

						case Gun.Type.Rifle:
						{
							data.damage_multiplier *= 0.96f;
							data.velocity_multiplier *= 0.97f;
							data.failure_rate = MathF.Max(0.00f, data.failure_rate - 0.002f);
							data.failure_rate *= 0.14f;
							data.cycle_interval *= 1.06f;
							data.stability *= 1.10f;
						}
						break;

						case Gun.Type.Shotgun:
						{
							data.damage_multiplier *= 0.95f;
							data.velocity_multiplier *= 0.95f;
							data.failure_rate = MathF.Max(0.00f, data.failure_rate - 0.001f);
							data.failure_rate *= 0.12f;
							data.cycle_interval += 0.03f;
							data.stability *= 1.15f;
						}
						break;

						case Gun.Type.SMG:
						{
							data.damage_multiplier *= 0.97f;
							data.velocity_multiplier *= 0.97f;
							data.failure_rate = MathF.Max(0.00f, data.failure_rate - 0.003f);
							data.failure_rate *= 0.17f;
							data.cycle_interval *= 1.06f;
							data.stability *= 1.20f;
						}
						break;

						default:
						{
							data.damage_multiplier *= 0.96f;
							data.velocity_multiplier *= 0.97f;
							data.failure_rate = MathF.Max(0.00f, data.failure_rate - 0.001f);
							data.failure_rate *= 0.20f;
							data.cycle_interval *= 1.06f;
							data.stability *= 1.17f;
						}
						break;
					}
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					for (var i = 0; i < context.requirements_old.Length; i++)
					{
						var requirement = context.requirements_old[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull())
							{
								if (material.flags.HasAll(Material.Flags.Manufactured))
								{
									context.requirements_new.Add(Crafting.Requirement.Resource(requirement.material, requirement.amount * 0.20f));
								}
								else if (material.flags.HasAll(Material.Flags.Metal))
								{
									context.requirements_new.Add(Crafting.Requirement.Resource(requirement.material, requirement.amount * 0.25f));
								}
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount *= 0.20f;
									context.requirements_new.Add(requirement);
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.amount *= 0.20f;
									context.requirements_new.Add(requirement);
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 0.05f;
									context.requirements_new.Add(requirement);
								}
								break;
							}
						}
					}

					for (var i = 0; i < context.requirements_new.Length; i++)
					{
						var requirement = context.requirements_new[i];

						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.difficulty += 2;
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.difficulty += 2;
								}
								break;
							}
						}

					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.tempered_frame",
				category: "Gun (Frame)",
				name: "Tempered Metal",
				description: "Greatly improves durability and stability of the gun.",

				//flags: Augment.Definition.Flags.Hidden,

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					offset.X.Clamp(context.rect.a.X, context.rect.b.X);
					offset.Y.Clamp(context.rect.a.Y, context.rect.b.Y);
					offset.Snap(0.125f, out offset);

					return true;
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.failure_rate *= 0.80f;
					data.stability *= 1.28f;

					ref var health = ref context.GetComponent<Health.Data>();
					if (!health.IsNull())
					{
						health.max *= 1.40f;
					}

					ref var armor = ref context.GetOrAddComponent<Armor.Data>();
					if (!armor.IsNull())
					{
						armor.material_type = Material.Type.Metal;
						armor.toughness += 70.00f;
					}
				},

				finalize: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					//data.failure_rate -= MathF.Min(data.failure_rate, 0.20f);
					//data.failure_rate *= 0.80f;
					//data.stability *= 1.10f;

					return true;
				},

				//#if CLIENT
				//				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				//				{
				//					ref var offset = ref handle.GetData<Vector2>();

				//					var size = GUI.Rm;
				//					size.X *= 0.50f;

				//					var dirty = false;
				//					dirty |= GUI.Picker("offset", size: size, ref offset, min: context.rect.a, max: context.rect.b);

				//					//dirty |= GUI.SliderFloat("X", ref offset.X, -0.50f, 0.50f, size: size);
				//					//GUI.SameLine();
				//					//dirty |= GUI.SliderFloat("Y", ref offset.Y, -0.20f, 0.10f, size: size);

				//					return dirty;
				//				},

				//				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				//				{
				//					ref var offset = ref handle.GetData<Vector2>();
				//					draw.DrawSprite("augment.tempered_frame", offset, scale: new(1.00f, 1.00f), pivot: new(0.50f, 0.50f));
				//				},
				//#endif

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					for (var i = 0; i < context.requirements_old.Length; i++)
					{
						var requirement = context.requirements_old[i];

						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 0.20f;
									requirement.difficulty += 8;
									context.requirements_new.Add(requirement);
								}
								break;
							}
						}
					}

					context.requirements_new.Add(Crafting.Requirement.Resource("phlogiston", 3.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.hardened_frame",
				category: "Gun (Frame)",
				name: "Hardened Frame",
				description: "Improves reliability of the gun.",
				flags: Augment.Definition.Flags.Hidden,

				//flags: Augment.Definition.Flags.Hidden,

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					offset.X.Clamp(context.rect.a.X, context.rect.b.X);
					offset.Y.Clamp(context.rect.a.Y, context.rect.b.Y);
					offset.Snap(0.125f, out offset);

					return true;
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.failure_rate *= 0.50f;
					data.stability *= 1.25f;
				},

				finalize: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.failure_rate -= MathF.Min(data.failure_rate, 0.20f);
					data.failure_rate *= 0.70f;
					data.stability *= 1.10f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 1.30f;
					}

					return true;
				},

				//#if CLIENT
				//				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				//				{
				//					ref var offset = ref handle.GetData<Vector2>();

				//					var size = GUI.Rm;
				//					size.X *= 0.50f;

				//					var dirty = false;
				//					dirty |= GUI.Picker("offset", size: size, ref offset, min: context.rect.a, max: context.rect.b);

				//					//offset.X.Clamp(-0.50f, 0.50f);
				//					//offset.Y.Clamp(-0.20f, 0.10f);

				//					//dirty |= GUI.SliderFloat("X", ref offset.X, -0.50f, 0.50f, size: size);
				//					//GUI.SameLine();
				//					//dirty |= GUI.SliderFloat("Y", ref offset.Y, -0.20f, 0.10f, size: size);

				//					return dirty;
				//				},

				//				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				//				{
				//					ref var offset = ref handle.GetData<Vector2>();
				//					draw.DrawSprite("augment.hardened_frame", offset, scale: new(1.00f, 1.00f), pivot: new(0.50f, 0.50f));
				//				},
				//#endif

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					for (var i = 0; i < context.requirements_old.Length; i++)
					{
						var requirement = context.requirements_old[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.flags.HasAll(Material.Flags.Manufactured))
							{
								context.requirements_new.Add(Crafting.Requirement.Resource(requirement.material, requirement.amount * 0.40f));
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 0.20f;
									context.requirements_new.Add(requirement);
								}
								break;

								case Work.Type.Machining:
								{
									requirement.amount *= 0.05f;
									context.requirements_new.Add(requirement);
								}
								break;
							}
						}
					}
				}
			));

			//			definitions.Add(Augment.Definition.New<Gun.Data>
			//			(
			//				identifier: "gun.casing.steel",
			//				category: "Gun (Frame)",
			//				name: "Steel Casing",
			//				description: "Improves reliability of the gun.",

			//				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//				{
			//					return augments.GetCount(handle) < 3;
			//				},

			//#if CLIENT
			//				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//				{
			//					ref var offset = ref handle.GetData<Vector2>();
			//					ref var modifier = ref handle.GetModifier();

			//					var dirty = false;

			//					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 9, size: new Vector2(GUI.RmX * 0.50f, GUI.RmY));
			//					GUI.SameLine();
			//					dirty |= GUI.Picker("offset", size: new Vector2(GUI.RmX, GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

			//					return dirty;
			//				},

			//				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
			//				{
			//					ref var offset = ref handle.GetData<Vector2>();
			//					ref var modifier = ref handle.GetModifier();
			//					var type = Maths.LerpInt(0, 9, modifier);

			//					var sprite = new Sprite("augment.casing.steel", 24, 16, (uint)type, 0);

			//					draw.DrawSprite(sprite, new Vector2(offset.X, offset.Y), pivot: new(0.50f, 0.50f));
			//				},
			//#endif

			//				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//				{
			//					for (var i = 0; i < context.requirements_old.Length; i++)
			//					{
			//						var requirement = context.requirements_old[i];

			//						if (requirement.type == Crafting.Requirement.Type.Resource)
			//						{
			//							ref var material = ref requirement.material.GetData();
			//							if (material.IsNotNull() && material.flags.HasAll(Material.Flags.Manufactured))
			//							{
			//								context.requirements_new.Add(Crafting.Requirement.Resource(requirement.material, requirement.amount * 0.40f));
			//							}
			//						}
			//						else if (requirement.type == Crafting.Requirement.Type.Work)
			//						{
			//							switch (requirement.work)
			//							{
			//								case Work.Type.Smithing:
			//								{
			//									requirement.amount *= 0.20f;
			//									context.requirements_new.Add(requirement);
			//								}
			//								break;

			//								case Work.Type.Machining:
			//								{
			//									requirement.amount *= 0.05f;
			//									context.requirements_new.Add(requirement);
			//								}
			//								break;
			//							}
			//						}
			//					}
			//				}
			//			));

			//			definitions.Add(Augment.Definition.New<Gun.Data>
			//			(
			//				identifier: "gun.framework.steel",
			//				category: "Gun (Frame)",
			//				name: "Steel Framework",
			//				description: "Improves reliability of the gun.",

			//				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//				{
			//					return augments.GetCount(handle) < 3;
			//				},

			//#if CLIENT
			//				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//				{
			//					ref var offset = ref handle.GetData<Vector2>();
			//					ref var modifier = ref handle.GetModifier();

			//					var dirty = false;

			//					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 9, size: new Vector2(GUI.RmX * 0.50f, GUI.RmY));
			//					GUI.SameLine();
			//					dirty |= GUI.Picker("offset", size: new Vector2(GUI.RmX, GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b);

			//					return dirty;
			//				},

			//				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
			//				{
			//					ref var offset = ref handle.GetData<Vector2>();
			//					ref var modifier = ref handle.GetModifier();
			//					var type = Maths.LerpInt(0, 9, modifier);

			//					var sprite = new Sprite("augment.framework.steel", 24, 16, (uint)type, 0);

			//					draw.DrawSprite(sprite, new Vector2(offset.X, offset.Y), pivot: new(0.50f, 0.50f));
			//				},
			//#endif

			//				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
			//				{
			//					for (var i = 0; i < context.requirements_old.Length; i++)
			//					{
			//						var requirement = context.requirements_old[i];

			//						if (requirement.type == Crafting.Requirement.Type.Resource)
			//						{
			//							ref var material = ref requirement.material.GetData();
			//							if (material.IsNotNull() && material.flags.HasAll(Material.Flags.Manufactured))
			//							{
			//								context.requirements_new.Add(Crafting.Requirement.Resource(requirement.material, requirement.amount * 0.40f));
			//							}
			//						}
			//						else if (requirement.type == Crafting.Requirement.Type.Work)
			//						{
			//							switch (requirement.work)
			//							{
			//								case Work.Type.Smithing:
			//								{
			//									requirement.amount *= 0.20f;
			//									context.requirements_new.Add(requirement);
			//								}
			//								break;

			//								case Work.Type.Machining:
			//								{
			//									requirement.amount *= 0.05f;
			//									context.requirements_new.Add(requirement);
			//								}
			//								break;
			//							}
			//						}
			//					}
			//				}
			//			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.reinforced_frame",
				category: "Gun (Frame)",
				name: "Reinforced Frame",
				description: "Increases weight, durability and stability.",
				flags: Augment.Definition.Flags.Hidden,

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();

					return GUI.SliderFloatLerp("Multiplier", ref modifier, 0.00f, 2.50f, size: GUI.Rm);
				},
#endif

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var modifier = ref handle.GetModifier();

					var mult = Maths.Lerp(0.00f, 2.50f, modifier);

					var extra_mass = 0.00f;

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.type == Material.Type.Metal && !material.flags.HasAny(Material.Flags.Manufactured))
							{
								var amount_added = requirement.amount * mult;
								requirement.amount += amount_added;
								extra_mass += amount_added * material.mass_per_unit;
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 1.00f + (mult * 0.25f);
								}
								break;
							}
						}
					}

					extra_mass += context.mass_old * mult * 0.20f;

					data.stability += extra_mass * 5.00f; // * Maths.Lerp(1.00f + (mult * 0.20f), data.stability * data.stability * 0.75f, 0.50f);

					ref var health = ref context.GetComponent<Health.Data>();
					if (!health.IsNull())
					{
						health.max += extra_mass * 50.00f;
					}

					ref var armor = ref context.GetComponent<Armor.Data>();
					if (!armor.IsNull())
					{
						armor.toughness *= 1.00f + (mult * 0.15f);
					}

					context.mass_new += extra_mass;
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.flimsy_frame",
				category: "Gun (Frame)",
				name: "Flimsy Frame",
				description: "Lowers the basic resource cost, but makes problems more pronounced.",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 0.00f, 1.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Value", ref value, 0.00f, 1.00f, size: GUI.Rm);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();

					data.failure_rate *= Maths.Lerp(1.20f, 1.80f, value);
					data.failure_rate += Maths.Lerp(0.03f, 0.10f, value);
					data.stability *= Maths.Clamp(0.99f - (data.failure_rate * 0.50f), 0.50f, 0.99f);
					data.cycle_interval *= 1.05f;
					data.reload_interval *= 1.10f;

					ref var health = ref context.GetComponent<Health.Data>();
					if (!health.IsNull())
					{
						health.max *= Maths.Lerp(0.95f, 0.70f, value);
					}
				},

				finalize: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();

					data.failure_rate += 0.03f;
					//data.stability *= Maths.Clamp(data.stability, 0.00f, Maths.Lerp(0.98f, 0.95f, value));

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 0.80f;
					}

					return true;
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && !material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= Maths.Lerp(0.80f, 0.60f, value);
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.simple_frame",
				category: "Gun (Frame)",
				name: "Simple Frame",
				description: "Simplifies the item, increasing reliability at cost of reduced performance.",

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.failure_rate *= 0.85f;
					data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
					data.stability *= 1.15f; // MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
					data.cycle_interval *= 1.35f;
					data.reload_interval *= 0.93f;
					data.damage_multiplier *= 0.98f;
				},

				finalize: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.failure_rate *= 0.60f;
					data.jitter_multiplier += 1.50f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 0.90f;
					}

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 0.90f;
									requirement.difficulty = (byte)(requirement.difficulty * 0.90f);
								}
								break;

								case Work.Type.Woodworking:
								{
									requirement.amount *= 0.85f;
									requirement.difficulty = (byte)(requirement.difficulty * 0.90f);
								}
								break;

								case Work.Type.Machining:
								{
									requirement.amount *= 0.65f;
									requirement.difficulty = (byte)(requirement.difficulty * 0.80f);
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 0.60f;
									requirement.difficulty = (byte)(requirement.difficulty * 0.75f);
								}
								break;
							}
						}
					}

					return true;
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= 0.78f;
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.revolver_gas_seal", // Should allow the use of silencer attachment on revolver in the future
				category: "Gun (Receiver)",
				name: "Gas Seal",
				description: "Increases damage and velocity by moving cylinder closer to barrel before each shot to avoid flash gap in revolver.",

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return data.feed == Gun.Feed.Cylinder && !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.failure_rate *= 0.85f;
					data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
					data.stability *= 1.34f; // MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
					data.damage_multiplier *= 1.10f;
					data.velocity_multiplier *= 1.05f;
					data.recoil_multiplier *= 1.10f;
				},

				finalize: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.failure_rate *= 0.60f;
					data.jitter_multiplier *= 0.90f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 1.05f;
					}

					return true;
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= 1.10f;
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 1.50f;
								}
								break;

								case Work.Type.Woodworking:
								{
									requirement.amount *= 1.05f;
								}
								break;

								case Work.Type.Machining:
								{
									requirement.amount *= 2.00f;
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 2.00f;
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.revolver_hand_fitted_parts",
				category: "Gun (Frame)",
				name: "Hand-Fitted Parts",
				description: "Increases damage and velocity by making flash gap in revolver shorter due to very careful construction, at the cost of workspeed.",

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return data.feed == Gun.Feed.Cylinder && !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.failure_rate *= 0.85f;
					data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
					data.stability *= 1.50f; // MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
					data.damage_multiplier *= 1.12f;
					data.velocity_multiplier *= 1.06f;
					//data.recoil_multiplier *= 1.12f;
				},

				finalize: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.failure_rate *= 0.60f;
					data.jitter_multiplier *= 0.90f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 1.05f;
					}

					return true;
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= 1.10f;
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 2.50f;
								}
								break;

								case Work.Type.Woodworking:
								{
									requirement.amount *= 1.05f;
								}
								break;

								case Work.Type.Machining:
								{
									requirement.amount *= 3.00f;
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 3.00f;
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.revolver_side_gate_loading",
				category: "Gun (Frame)",
				name: "Side Gate Loading",
				description: "Simplifies revolver's construction, increasing reliability at the cost of reload speed.",

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return data.feed == Gun.Feed.Cylinder && !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.failure_rate *= 0.85f;
					data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
					data.stability *= 1.15f; // MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
					data.reload_interval *= 2.00f;
				},

				finalize: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.failure_rate *= 0.60f;
					data.jitter_multiplier *= 0.90f;

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 0.95f;
					}

					return true;
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= 0.90f;
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Smithing:
								{
									requirement.amount *= 0.60f;
								}
								break;

								case Work.Type.Woodworking:
								{
									requirement.amount *= 0.95f;
								}
								break;

								case Work.Type.Machining:
								{
									requirement.amount *= 0.60f;
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 0.60f;
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.faster_cycling_mechanism",
				category: "Gun (Receiver)",
				name: "Faster Cycling",
				description: "Increases rate of fire, but lowers reliability.",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 0.00f, 1.00f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Value", ref value, 0.00f, 1.00f, size: GUI.Rm);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					var ratio = value;

					switch (data.action)
					{
						case Gun.Action.Gas:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 2.00f, ratio);
							data.failure_rate += Maths.Lerp(0.00f, 0.02f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.35f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 1.40f, ratio);
						}
						break;

						case Gun.Action.Blowback:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 5.50f, ratio);
							data.failure_rate += Maths.Lerp(0.00f, 0.30f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.55f, ratio);
							data.jitter_multiplier *= Maths.Lerp(1.00f, 1.60f, ratio);
						}
						break;

						case Gun.Action.Revolver:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 1.80f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.67f, ratio);
							data.reload_interval += Maths.Lerp(0.00f, 0.04f, ratio);
						}
						break;

						case Gun.Action.Bolt:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 1.85f, ratio);
							data.failure_rate += Maths.Lerp(0.00f, 0.01f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.67f, ratio);
						}
						break;

						case Gun.Action.Pump:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 1.80f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.75f, ratio);
						}
						break;

						case Gun.Action.Crank:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 1.90f, ratio);
							data.failure_rate += Maths.Lerp(0.00f, 0.05f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.65f, ratio);
						}
						break;

						case Gun.Action.Manual:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 1.80f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.75f, ratio);
						}
						break;

						case Gun.Action.Lever:
						{
							data.failure_rate *= Maths.Lerp(1.00f, 1.80f, ratio);
							data.cycle_interval *= Maths.Lerp(1.00f, 0.75f, ratio);
						}
						break;
					}

					//if (data.flags.HasAll(Gun.Flags.Automatic))
					//{
					//	data.failure_rate *= Maths.Lerp(1.00f, 2.50f, ratio);
					//}

					//data.stability = Maths.Clamp(data.stability - MathF.Min(data.stability, Maths.Lerp(0.00f, (1.00f / data.cycle_interval) * 0.15f, ratio)), 0.00f, 1.00f);
					data.failure_rate += Maths.Lerp(0.00f, (1.00f / data.cycle_interval) * 0.35f, ratio);
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					var h_mat_machine_parts = new IMaterial.Handle("machine_parts");

					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount += 80.00f;
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount += 60.00f;
								}
								break;
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							if (requirement.material == h_mat_machine_parts)
							{
								requirement.amount *= 1.20f;
							}
						}
					}

					context.requirements_new.Add(Crafting.Requirement.Resource("lubricant", 5.00f));
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.slower_cycling_mechanism",
				category: "Gun (Receiver)",
				name: "Slower Cycling",
				description: "Decreases fire rate, but increases reliability.",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 1.00f, 2.00f);

					return true;
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Value", ref value, 1.00f, 2.00f, size: GUI.Rm);
				},
#endif

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					var ratio = value - 1.00f;

					switch (data.type)
					{
						case Gun.Type.Handgun:
						{
							var mult = 1.00f - ratio;

							data.failure_rate *= 0.85f;
							data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
							//data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
							data.cycle_interval *= Maths.Lerp(1.20f, 10.00f, ratio);
						}
						break;

						case Gun.Type.Rifle:
						{
							var mult = 1.00f - ratio;

							data.failure_rate *= 0.85f;
							data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
							//data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
							data.cycle_interval *= Maths.Lerp(1.20f, 5.00f, ratio);
						}
						break;

						case Gun.Type.Shotgun:
						{
							data.failure_rate *= 0.85f;
							data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
							//data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
							data.cycle_interval *= Maths.Lerp(1.20f, 5.00f, ratio);
						}
						break;

						case Gun.Type.SMG:
						{
							data.failure_rate *= 0.85f;
							data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
							//data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
							data.cycle_interval *= Maths.Lerp(1.20f, 10.00f, ratio);
						}
						break;

						default:
						{
							data.failure_rate *= 0.85f;
							data.failure_rate -= MathF.Min(data.failure_rate, 0.05f);
							//data.stability *= MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
							data.cycle_interval *= Maths.Lerp(1.20f, 10.00f, ratio);
						}
						break;
					}

					switch (data.action)
					{
						case Gun.Action.Blowback:
						{
							data.cycle_interval *= 1.20f;
						}
						break;

						case Gun.Action.Gas:
						{
							data.cycle_interval *= 1.14f;
						}
						break;
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.gas_operation",
				category: "Gun (Receiver)",
				name: "Action: Gas-Operated",
				description: "Converts to gas-operated action.",

				requirements: new()
				{
					[0] = Crafting.Requirement.Level(Experience.Type.Engineering, 10, Crafting.Requirement.Flags.No_Consume),
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return data.action == Gun.Action.Blowback;
				},

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					switch (data.action)
					{
						case Gun.Action.Blowback:
						{
							//data.damage_multiplier *= 1.25f; Too OP

							data.failure_rate *= 0.30f;
							data.cycle_interval *= 0.90f;

							//data.stability += MathF.Min(data.stability, 0.15f);
							//data.stability = Maths.Clamp(data.stability * 1.30f, 0.00f, 1.00f);

							data.recoil_multiplier *= 0.90f;
							data.reload_interval *= 0.80f;

							data.action = Gun.Action.Gas;

							ref var body = ref context.GetComponent<Body.Data>();
							if (!body.IsNull())
							{
								body.mass_multiplier *= 0.90f;
							}
						}
						break;
					}
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull())
							{
								if (material.flags.HasAll(Material.Flags.Manufactured))
								{
									requirement.amount *= 0.90f;
								}
								else if (material.flags.HasAll(Material.Flags.Metal))
								{
									requirement.amount *= 0.85f;
								}
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount *= 1.10f;
									requirement.amount += 50.00f;
									requirement.difficulty += 2;
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.amount *= 1.40f;
									requirement.difficulty += 3;
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 0.70f;
									requirement.difficulty += 2;
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.single_shot",
				category: "Gun (Receiver)",
				name: "Feed: Single-Shot",
				description: "Converts to single-shot feed mechanism.",

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return data.feed != Gun.Feed.Single && data.feed != Gun.Feed.Breech && data.feed != Gun.Feed.Front && !augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					data.feed = Gun.Feed.Single;
					data.flags.SetFlag(Gun.Flags.Full_Reload, false);
					data.flags.SetFlag(Gun.Flags.Automatic, false);
					data.max_ammo = 1;

					if (data.ammo_filter.HasAll(Material.Flags.Ammo_Shell))
					{
						data.reload_interval = 3.00f;
						data.sound_reload = "cannon_reload";
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_AC))
					{
						data.reload_interval = 0.50f;
						data.sound_reload = "musket_reload";
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_Rocket))
					{
						data.reload_interval = 2.00f;
						data.sound_reload = "bazooka_reload";
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_MG))
					{
						data.reload_interval = 0.50f;
						data.sound_reload = "musket_reload";
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_SG))
					{
						data.reload_interval = 0.50f;
						data.sound_reload = "scattergun_reload";
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_HC))
					{
						data.reload_interval = 0.50f;
						data.sound_reload = "rifle_reload";
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_LC))
					{
						data.reload_interval = 0.50f;
						data.sound_reload = "revolver_reload";
					}
				},

				finalize: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					//data.failure_rate = Maths.Clamp(data.failure_rate * 0.10f + (1.00f - Maths.Clamp(data.stability, 0.00f, 1.00f)), 0.00f, 1.00f);
					data.failure_rate = Maths.Clamp(data.failure_rate * 0.10f, 0.00f, 1.00f);
					data.stability *= 1.70f;

					return true;
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					for (var i = 0; i < context.requirements_new.Length; i++)
					{
						var requirement = context.requirements_new[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.flags.HasAll(Material.Flags.Manufactured))
							{
								requirement.amount *= 0.50f;
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount *= 0.50f;
									requirement.difficulty = (byte)(requirement.difficulty * 0.20f);
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.amount *= 0.65f;
									requirement.difficulty = (byte)(requirement.difficulty * 0.20f);
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 0.30f;
									requirement.difficulty = (byte)(requirement.difficulty * 0.20f);
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.extra_barrel",
				category: "Gun (Barrel)",
				name: "Extra Barrel",
				description: "Adds an extra barrel to the gun.",

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return data.type != Gun.Type.Cannon && data.type != Gun.Type.Launcher && augments.GetCount(handle) < 3;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var simultaneous = ref handle.GetData<bool>();
					return GUI.Checkbox("Simultaneous Fire", ref simultaneous, GUI.Rm);
				},

				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					var count = augments.GetCount(handle);

					var sprite = new Sprite("augment.extra_barrel", 15, 15, (uint)(count - 1), 0);

					//draw.DrawSprite("gun.extra_barrel", data.muzzle_offset, pivot: new(0.50f, 0.50f));
					draw.DrawSprite(sprite, Maths.Snap(data.muzzle_offset, 0.125f), pivot: new(0.50f, 0.50f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var simultaneous = ref handle.GetData<bool>();
					data.barrel_count++;

					if (simultaneous)
					{
						data.ammo_per_shot += 1.00f;
						data.smoke_amount += 1.50f;
						data.projectile_count += 1;
					}

					switch (data.feed)
					{
						case Gun.Feed.Breech:
						case Gun.Feed.Front:
						case Gun.Feed.Single:
						{
							if (simultaneous)
							{
								data.failure_rate *= 1.10f;
								data.failure_rate += 0.25f;

								//data.stability -= MathF.Min(data.stability, 0.50f);
								//data.stability = Maths.Clamp(data.stability * 0.60f, 0.00f, 1.00f);

								data.stability *= 1.24f;

								//data.reload_interval *= 1.30f;

								data.jitter_multiplier *= 1.30f;
								data.jitter_multiplier += 5.00f;

								data.sound_volume *= 1.50f;
								data.sound_pitch *= 0.80f;
								//data.recoil_multiplier *= 2.00f;
							}
							else
							{
								data.stability *= 1.24f;
								//data.stability -= MathF.Min(data.stability, 0.02f);
								//data.stability = MathF.Pow(Maths.Clamp(data.stability, 0.00f, 1.00f), 2.00f);
							}
						}
						break;

						default:
						{
							if (simultaneous)
							{
								data.failure_rate *= 1.70f;
								data.failure_rate += 0.20f;

								//data.stability -= MathF.Min(data.stability, 0.30f);
								//data.stability = Maths.Clamp(data.stability * 0.70f, 0.00f, 1.00f);

								//data.stability *= 0.70f;
								data.stability *= 1.12f;

								data.cycle_interval *= 1.30f;
								data.reload_interval *= 1.30f;

								data.jitter_multiplier *= 2.30f;
								data.jitter_multiplier += 3.00f;

								data.sound_volume *= 1.50f;
								data.sound_pitch *= 0.80f;
								//data.recoil_multiplier *= 2.00f;
							}
							else
							{
								data.failure_rate *= 3.50f;
								data.failure_rate += 0.05f;

								//data.stability -= MathF.Min(data.stability, 0.10f);
								//data.stability = Maths.Clamp(data.stability * 0.95f, 0.00f, 1.00f);

								data.stability *= 1.12f;

								data.reload_interval *= 1.30f;

								data.jitter_multiplier *= 2.50f;
								data.jitter_multiplier += 3.00f;

								data.cycle_interval *= 0.70f;
							}
						}
						break;
					}
				},

				finalize: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var simultaneous = ref handle.GetData<bool>();

					if (data.feed == Gun.Feed.Single || data.feed == Gun.Feed.Breech || data.feed == Gun.Feed.Front)
					{
						data.max_ammo += 1;
					}

					if (simultaneous)
					{

					}
					else
					{

					}

					context.mass_new += context.mass_old * 0.10f;

					//var barrel_count_inv = MathF.ReciprocalEstimate(MathF.Max(data.barrel_count - augments.GetCount(handle), 1));



					//ref var body = ref context.GetComponent<Body.Data>();
					//if (!body.IsNull())
					//{
					//	body.mass_multiplier += 0.30f * barrel_count_inv;
					//}

					return true;
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var overheat = ref context.GetComponent<Overheat.Data>();
					if (!overheat.IsNull())
					{
						overheat.cool_rate *= 1.30f;
					}

					var barrel_count_inv = MathF.ReciprocalEstimate(MathF.Max(data.barrel_count - augments.GetCount(handle), 1));

					for (var i = 0; i < context.requirements_old.Length; i++)
					{
						var requirement = context.requirements_old[i];

						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							ref var material = ref requirement.material.GetData();
							if (material.IsNotNull() && material.flags.HasAll(Material.Flags.Metal))
							{
								context.requirements_new.Add(Crafting.Requirement.Resource(requirement.material, requirement.amount * 0.40f * barrel_count_inv));
							}
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							switch (requirement.work)
							{
								case Work.Type.Machining:
								{
									requirement.amount *= 0.50f * barrel_count_inv;
									context.requirements_new.Add(requirement);
								}
								break;

								case Work.Type.Smithing:
								{
									requirement.amount *= 0.75f * barrel_count_inv;
									context.requirements_new.Add(requirement);
								}
								break;

								case Work.Type.Assembling:
								{
									requirement.amount *= 0.70f * barrel_count_inv;
									context.requirements_new.Add(requirement);
								}
								break;
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.flared_barrel",
				category: "Gun (Barrel)",
				name: "Flared Barrel",
				description: "Increases spread and loudness, but also greatly reduces recoil.",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var amount = ref handle.GetData<float>();
					amount = Maths.Clamp(amount, 0.00f, 1.00f);

					return true;
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();
					return GUI.SliderFloat("Value", ref value, 0.00f, 1.00f, size: GUI.Rm);
				},

				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var value = ref handle.GetData<float>();
					draw.DrawSprite("augment.flared_barrel", Maths.Snap(data.muzzle_offset, 0.125f), scale: new(1.00f, 0.50f + (value * 0.50f)), pivot: new(0.50f, 0.50f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var value = ref handle.GetData<float>();

					data.damage_multiplier *= Maths.Lerp(0.90f, 0.60f, value);

					data.flash_size *= Maths.Lerp(0.80f, 0.90f, value);
					data.velocity_multiplier *= Maths.Lerp(0.90f, 0.50f, value);

					data.jitter_multiplier *= Maths.Lerp(1.10f, 1.50f, value);
					data.jitter_multiplier += Maths.Lerp(2.50f, 3.00f, value);
					data.sound_volume *= Maths.Lerp(1.15f, 1.35f, value);
					data.sound_size *= Maths.Lerp(1.10f, 1.30f, value);
					data.sound_pitch *= Maths.Lerp(0.98f, 0.96f, value);
					data.recoil_multiplier = MathF.Max(0.15f, data.recoil_multiplier * Maths.Lerp(0.80f, 0.50f, value));
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.bayonet",
				category: "Gun (Barrel)",
				name: "Add: Bayonet",
				description: "Attach a bayonet to the gun, giving it a secondary melee attack.",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					offset.Snap(0.125f, out offset);
					offset.X.Clamp(-0.50f, 0.50f);
					offset.Y.Clamp(0.00f, 1.00f);

					return true;
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle) && !context.HasComponent<Melee.Data>();
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					var size = GUI.Rm;
					size.X *= 0.50f;

					var dirty = false;
					dirty |= GUI.Picker("offset", "Offset", size: size, ref offset, min: new Vector2(-0.50f, 0.00f), max: new Vector2(0.50f, 1.00f));

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					draw.DrawSprite("augment.bayonet", Maths.Snap(data.muzzle_offset + offset, 0.125f), scale: new(1.00f, 1.00f), pivot: new(0.50f, 0.50f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					ref var melee = ref context.GetOrAddComponent<Melee.Data>();
					if (!melee.IsNull())
					{
						melee.attack_type = Melee.AttackType.Thrust;
						melee.aoe = 0.250f;
						melee.thickness = 0.125f;
						melee.category = Melee.Category.Pointed;
						melee.cooldown = 1.00f;
						melee.damage_base = 125.00f;
						melee.damage_bonus = 350.00f;
						melee.damage_type = Damage.Type.Stab;
						melee.knockback = 75.00f;
						melee.max_distance = 1.90f;
						melee.sound_swing = "tool_swing_00";
						melee.hit_mask = Physics.Layer.World | Physics.Layer.Destructible;
						melee.hit_require = Physics.Layer.Solid;
						melee.hit_exclude = Physics.Layer.Ignore_Melee | Physics.Layer.Decoration | Physics.Layer.Tree;
						melee.flags |= Melee.Flags.Use_RMB;
						melee.hit_offset = data.muzzle_offset + offset;
					}

					ref var melee_state = ref context.GetOrAddComponent<Melee.State>();
					if (!melee_state.IsNull())
					{

					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.silencer",
				category: "Gun (Barrel)",
				name: "Silencer",
				description: "Attach a silencer to the gun. Reduces sound intensity, muzzle velocity and recoil.",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					offset.Snap(0.125f, out offset);
					offset.X.Clamp(-0.25f, 0.25f);
					offset.Y.Clamp(-0.125f, 0.125f);

					return true;
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return data.ammo_filter.HasAny(Material.Flags.Ammo_MG | Material.Flags.Ammo_SG | Material.Flags.Ammo_HC | Material.Flags.Ammo_LC) && !augments.HasAugment(handle);
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					var size = GUI.Rm;
					size.X *= 0.50f;

					var dirty = false;
					dirty |= GUI.Picker("offset", "Offset", size: size, ref offset, min: new Vector2(-0.25f, -0.125f), max: new Vector2(0.25f, 0.125f));

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					var frame_y = 0u;

					if (data.ammo_filter.HasAll(Material.Flags.Ammo_MG))
					{
						frame_y = 3u;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_SG))
					{
						frame_y = 2u;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_HC))
					{
						frame_y = 1u;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_LC))
					{
						frame_y = 0u;
					}

					var sprite = new Sprite("augment.silencer", 16, 8, 0, frame_y);

					draw.DrawSprite(sprite, Maths.Snap(data.muzzle_offset + offset, 0.125f), scale: new(1.00f, 1.00f), pivot: new(0.25f, 0.50f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					data.sound_pitch *= 1.60f;
					data.sound_volume *= 0.75f;
					data.sound_size += 4.00f; // Diffuses the sound
					data.sound_dist_multiplier *= 0.65f;
					data.flash_size *= 0.65f;

					data.velocity_multiplier *= 0.90f;
					data.damage_multiplier *= 0.90f;
					data.recoil_multiplier *= 0.90f;

					if (data.ammo_filter.HasAll(Material.Flags.Ammo_MG))
					{
						data.recoil_multiplier *= 0.75f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_SG))
					{
						data.recoil_multiplier *= 0.80f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_HC))
					{
						data.recoil_multiplier *= 0.85f;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_LC))
					{
						data.recoil_multiplier *= 0.90f;
					}

					context.mass_new += 0.50f;

					//App.WriteLine("test9");
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("steel.ingot", 1.00f));
					context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 150.00f, 10));
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.axe",
				category: "Gun (Barrel)",
				name: "Add: Axe",
				description: "Attach an axe blade to the gun, giving it a secondary melee attack.",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					offset.Snap(0.125f, out offset);
					offset.X.Clamp(-0.50f, 0.50f);
					offset.Y.Clamp(0.00f, 1.00f);

					return true;
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle) && !context.HasComponent<Melee.Data>();
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					var size = GUI.Rm;
					size.X *= 0.50f;

					var dirty = false;
					dirty |= GUI.Picker("offset", "Offset", size: size, ref offset, min: new Vector2(-0.50f, 0.00f), max: new Vector2(0.50f, 1.00f));

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					draw.DrawSprite("augment.axe", Maths.Snap(data.muzzle_offset + offset, 0.125f), scale: new(1.00f, 1.00f), pivot: new(0.50f, 0.50f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();

					ref var melee = ref context.GetOrAddComponent<Melee.Data>();
					if (!melee.IsNull())
					{
						melee.attack_type = Melee.AttackType.Swing;
						melee.aoe = 0.500f;
						melee.thickness = 1.75f;
						melee.category = Melee.Category.Bladed;
						melee.cooldown = 2.00f;
						melee.damage_base = 275.00f;
						melee.damage_bonus = 125.00f;
						melee.damage_type = Damage.Type.Axe;
						melee.knockback = 100.00f;
						melee.max_distance = 1.30f;
						melee.sound_swing = "tool_swing_00";
						melee.hit_mask = Physics.Layer.World | Physics.Layer.Destructible;
						melee.hit_exclude = Physics.Layer.Ignore_Melee | Physics.Layer.Decoration | Physics.Layer.Tree;
						melee.flags |= Melee.Flags.Use_RMB;
						melee.hit_offset = data.muzzle_offset + offset;
						//melee.swing_offset = new Vector2(-1, 1);
						melee.swing_offset = new Vector2(-1.00f, 0.00f);
						melee.swing_rotation = -1.50f;
						melee.hit_direction = new Vector2(0.00f, 1.00f);
					}

					ref var melee_state = ref context.GetOrAddComponent<Melee.State>();
					if (!melee_state.IsNull())
					{

					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.automatic_reloading",
				category: "Gun (Ammo)",
				name: "ARC-MT Auto-Loader",
				description: "Automatically reloads the weapon once the magazine is empty.",

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle) && data.type != Gun.Type.Handgun;
				},

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					offset.X.Clamp(data.receiver_offset.X - 0.250f, MathF.Min(data.receiver_offset.X + 1.250f, data.muzzle_offset.X - 0.500f));
					offset.Y.Clamp(data.receiver_offset.Y - 0.125f, data.receiver_offset.Y + 0.250f);

					return true;
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					//ref var offset = ref handle.GetData<Vector2>();

					//var dirty = false;

					//dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmX, GUI.RmY), ref offset, min: context.rect.a, max: context.rect.b, sensitivity: 0.01f);

					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;
					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 3, size: new Vector2(GUI.RmX - (GUI.RmY * 2), GUI.RmY));
					GUI.SameLine();
					//dirty |= GUI.Checkbox("mirror_x", ref handle.flags, Augment.Handle.Flags.Mirror_X, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					//GUI.SameLine();
					dirty |= GUI.Checkbox("mirror_y", ref handle.flags, Augment.Handle.Flags.Mirror_Y, size: new Vector2(GUI.RmY), show_text: false, show_tooltip: true);
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset,
						min: new Vector2(data.receiver_offset.X - 0.250f, data.receiver_offset.Y - 0.125f),
						max: new Vector2(MathF.Min(data.receiver_offset.X + 1.250f, data.muzzle_offset.X - 0.500f), data.receiver_offset.Y + 0.250f));

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 3, modifier);

					var frame_y = 0u;

					if (data.ammo_filter.HasAll(Material.Flags.Ammo_Shell))
					{
						frame_y = 6;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_AC))
					{
						frame_y = 5;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_Rocket))
					{
						frame_y = 4;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_MG))
					{
						frame_y = 3;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_SG))
					{
						frame_y = 2;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_HC))
					{
						frame_y = 1;
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_LC))
					{
						frame_y = 0;
					}

					var sprite = new Sprite("augment.auto_loader", 24, 16, (uint)type, frame_y);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), pivot: new(0.50f, 0.50f), scale: handle.GetScale());
					//draw.DrawSprite(sprite, new Vector2(offset.X, offset.Y + data.muzzle_offset.Y), scale: new(offset.X > 0.00f ? -1.00f : 1.00f, offset.Y > 0.00f ? 1.00f : -1.00f), pivot: new(0.50f, 0.50f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var automatic_reload = ref context.GetOrAddComponent<AutomaticReload.Data>();

					data.reload_interval = 2.70f;
					data.flags |= Gun.Flags.Full_Reload;
					data.sound_reload = "gun.reload.arcane.00";
				},

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 3, modifier);

					var extra_mass = 0.00f;

					if (data.ammo_filter.HasAll(Material.Flags.Ammo_Shell))
					{
						context.requirements_new.Add(Crafting.Requirement.Resource("arcane_actuator", 10.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Resource("steel.ingot", 95.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Resource("machine_parts", 25.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 500.00f, 20));
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_AC))
					{
						context.requirements_new.Add(Crafting.Requirement.Resource("arcane_actuator", 5.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Resource("steel.ingot", 60.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Resource("machine_parts", 15.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 250.00f, 20));
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_Rocket))
					{
						context.requirements_new.Add(Crafting.Requirement.Resource("arcane_actuator", 1.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Resource("steel.ingot", 40.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Resource("machine_parts", 17.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 100.00f, 10));
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_MG))
					{
						context.requirements_new.Add(Crafting.Requirement.Resource("arcane_actuator", 4.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Resource("steel.ingot", 20.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Resource("machine_parts", 13.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 200.00f, 20));
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_SG))
					{
						context.requirements_new.Add(Crafting.Requirement.Resource("arcane_actuator", 2.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Resource("steel.ingot", 16.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Resource("machine_parts", 12.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 100.00f, 20));
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_HC))
					{
						switch (type)
						{
							case 0:
							{
								context.requirements_new.Add(Crafting.Requirement.Resource("arcane_actuator", 1.00f), ref extra_mass);
								context.requirements_new.Add(Crafting.Requirement.Resource("steel.ingot", 1.00f), ref extra_mass);
								context.requirements_new.Add(Crafting.Requirement.Resource("machine_parts", 5.00f), ref extra_mass);
								context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 350.00f, 20));
							}
							break;

							default:
							{
								context.requirements_new.Add(Crafting.Requirement.Resource("arcane_actuator", 2.00f), ref extra_mass);
								context.requirements_new.Add(Crafting.Requirement.Resource("steel.ingot", 13.00f), ref extra_mass);
								context.requirements_new.Add(Crafting.Requirement.Resource("machine_parts", 8.00f), ref extra_mass);
								context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 100.00f, 20));
							}
							break;
						}
					}
					else if (data.ammo_filter.HasAll(Material.Flags.Ammo_LC))
					{
						context.requirements_new.Add(Crafting.Requirement.Resource("arcane_actuator", 1.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Resource("steel.ingot", 5.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Resource("machine_parts", 4.00f), ref extra_mass);
						context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 100.00f, 15));
					}

					context.mass_new += extra_mass;
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.recoil_compensator",
				category: "Gun (Frame)",
				name: "ARC-MT Compensator",
				description: "Applies force in opposite direction upon being fired. Needs careful calibration.",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					offset.X.Clamp(-0.375f, data.muzzle_offset.X - 0.250f);
					offset.Y.Clamp(-0.125f, 0.250f);

					return true;
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

#if CLIENT
				//draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				//{
				//	ref var value = ref handle.GetData<Vector2>();
				//	var dirty = false;

				//	var size = GUI.Rm;

				//	dirty |= GUI.SliderFloat("Count", ref value.X, 1.00f, 4.00f, snap: 1.00f, size: new(size.X * 0.50f, size.Y));
				//	GUI.SameLine();
				//	dirty |= GUI.SliderFloat("Adjustment", ref value.Y, -2.00f, 2.00f, snap: 0.01f, size: new(size.X * 0.50f, size.Y));

				//	return dirty;
				//},

				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;
					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 3, size: GUI.GetRemainingSpace(x: -GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset, min: new Vector2(-0.375f, -0.125f), max: new Vector2(context.rect.b.X, 0.250f));

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 3, modifier);

					var frame_y = 0u;

					if (data.ammo_filter.HasAny(Material.Flags.Ammo_LC))
					{
						frame_y = 0u;
					}
					else if (data.ammo_filter.HasAny(Material.Flags.Ammo_HC | Material.Flags.Ammo_SG | Material.Flags.Ammo_Musket))
					{
						frame_y = 1u;
					}
					else if (data.ammo_filter.HasAny(Material.Flags.Ammo_MG | Material.Flags.Ammo_Rocket))
					{
						frame_y = 2u;
					}
					else if (data.ammo_filter.HasAny(Material.Flags.Ammo_Shell | Material.Flags.Ammo_AC))
					{
						frame_y = 3u;
					}

					var sprite = new Sprite("augment.compensator", 16, 16, (uint)type, frame_y);

					draw.DrawSprite(sprite, Maths.Snap(new Vector2(data.receiver_offset.X + 0.125f, data.muzzle_offset.Y + 0.125f) + offset, 0.125f), scale: new(1.00f, 1.00f), pivot: new(1.00f, 0.50f));
				},
#endif

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					var h_essence = new IEssence.Handle("motion");
					ref var essence_data = ref h_essence.GetData();
					if (essence_data.IsNotNull())
					{
						ref var offset = ref handle.GetData<Vector2>();
						ref var modifier = ref handle.GetModifier();
						var type = Maths.LerpInt(0, 3, modifier);

						var mass_added = 0.00f;

						//var mass = context.base_mass;

						//ref var body = ref context.GetComponent<Body.Data>();
						//if (!body.IsNull())
						//{
						//	mass += body.mass_extra;
						//}

						//data.recoil_multiplier -= Maths.Clamp(MathF.Abs((1.00f / MathF.Max(data.recoil_multiplier * value.Y, 0.10f)) * value.Y), data.recoil_multiplier * 0.20f, data.recoil_multiplier * 2.50f) * value.X * 0.80f; // MathF.Max(data.recoil_multiplier * 0.20f, 0.10f);

						var pellet_count = 0.00f;
						var smirglum_count = 0.00f;

						if (data.ammo_filter.HasAny(Material.Flags.Ammo_LC))
						{
							pellet_count = (1.00f + type) * 0.250f;
							smirglum_count = (1.00f + type) * 0.50f;
						}
						else if (data.ammo_filter.HasAny(Material.Flags.Ammo_HC | Material.Flags.Ammo_SG | Material.Flags.Ammo_Musket))
						{
							pellet_count = (1.00f + type) * 1.00f;
							smirglum_count = (1.00f + type) * 1.50f;
						}
						else if (data.ammo_filter.HasAny(Material.Flags.Ammo_MG | Material.Flags.Ammo_Rocket))
						{
							pellet_count = (1.00f + type) * 3.00f;
							smirglum_count = (1.00f + type) * 3.00f;
						}
						else if (data.ammo_filter.HasAny(Material.Flags.Ammo_Shell | Material.Flags.Ammo_AC))
						{
							pellet_count = (1.00f + type) * 5.00f;
							smirglum_count = (1.00f + type) * 7.00f;
						}

						context.requirements_new.Add(Crafting.Requirement.Resource("pellet.motion", pellet_count), ref mass_added);
						context.requirements_new.Add(Crafting.Requirement.Resource("smirglum.ingot", smirglum_count), ref mass_added);
						context.requirements_new.Add(Crafting.Requirement.Resource("phlogiston", Maths.SnapCeil(smirglum_count * 0.25f, 0.50f)));
						context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 150.00f * smirglum_count * (type + pellet_count) * 0.25f, (byte)(10 + ((type + pellet_count).Pow2()))));

						context.mass_new += mass_added;

						var force = essence_data.force_emit * pellet_count;
						var dist_mult = 1.00f / (1.00f + (Vector2.DistanceSquared(offset, new Vector2(0.125f, 0.125f)) * 10.00f));

						ref var recoil_compensator = ref context.GetOrAddComponent<RecoilCompensator.Data>();
						if (recoil_compensator.IsNotNull())
						{
							recoil_compensator.offset = data.receiver_offset + offset;
							recoil_compensator.modifier = MathF.ReciprocalEstimate(1.00f + (type * 0.50f));
							recoil_compensator.force = force * dist_mult;
							//recoil_compensator.ratio = Maths.Normalize(Maths.Normalize(force * dist_mult, context.mass_new * 20.00f), data.recoil_multiplier);
						}

						ref var light = ref context.GetOrAddTrait<RecoilCompensator.Data, Light.Data>();
						if (light.IsNotNull())
						{
							light.color = essence_data.color_emit;
							light.offset = data.receiver_offset + offset - new Vector2(0.750f, 0.00f);
							light.scale = new(2, 1);
							light.intensity = 1.500f;
							light.texture = Light.tex_light_box_00;
						}

						ref var holdable = ref context.GetComponent<Holdable.Data>();
						if (holdable.IsNotNull())
						{
							//holdable.offset.X = Maths.Lerp(holdable.offset.X, data.receiver_offset.X + recoil_compensator.offset.X - 0.500f, 0.500f);
						}

						//data.recoil_multiplier -= Maths.Normalize(force * dist_mult, context.mass_new * 20.00f);
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.accelerator",
				category: "Gun (Barrel)",
				name: "ARC-MT Accelerator",
				description: "Greatly increases projectile velocity, as well as recoil.",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					offset.X.Clamp(data.receiver_offset.X, data.muzzle_offset.X);
					offset.Y.Clamp(data.receiver_offset.Y, data.receiver_offset.Y);

					return true;
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

#if CLIENT
				//draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				//{
				//	ref var value = ref handle.GetData<Vector2>();
				//	var dirty = false;

				//	var size = GUI.Rm;

				//	dirty |= GUI.SliderFloat("Count", ref value.X, 1.00f, 4.00f, snap: 1.00f, size: new(size.X * 0.50f, size.Y));
				//	GUI.SameLine();
				//	dirty |= GUI.SliderFloat("Adjustment", ref value.Y, -2.00f, 2.00f, snap: 0.01f, size: new(size.X * 0.50f, size.Y));

				//	return dirty;
				//},

				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;
					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 3, size: GUI.GetRemainingSpace(x: -GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset, min: new Vector2(data.receiver_offset.X, data.receiver_offset.Y), max: new Vector2(data.muzzle_offset.X, data.receiver_offset.Y));

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 3, modifier);

					var frame_y = 0u;

					if (data.ammo_filter.HasAny(Material.Flags.Ammo_LC))
					{
						frame_y = 0u;
					}
					else if (data.ammo_filter.HasAny(Material.Flags.Ammo_HC | Material.Flags.Ammo_SG | Material.Flags.Ammo_Musket))
					{
						frame_y = 1u;
					}
					else if (data.ammo_filter.HasAny(Material.Flags.Ammo_MG | Material.Flags.Ammo_Rocket))
					{
						frame_y = 2u;
					}
					else if (data.ammo_filter.HasAny(Material.Flags.Ammo_Shell | Material.Flags.Ammo_AC))
					{
						frame_y = 3u;
					}

					var sprite = new Sprite("augment.accelerator", 16, 16, (uint)type, frame_y);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), scale: new(1.00f, 1.00f), pivot: new(1.00f, 0.50f));
				},
#endif

				apply_1: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					var h_essence = new IEssence.Handle("motion");
					ref var essence_data = ref h_essence.GetData();
					if (essence_data.IsNotNull())
					{
						ref var offset = ref handle.GetData<Vector2>();
						ref var modifier = ref handle.GetModifier();
						var type = Maths.LerpInt(0, 3, modifier);

						var mass_added = 0.00f;

						//var mass = context.base_mass;

						//ref var body = ref context.GetComponent<Body.Data>();
						//if (!body.IsNull())
						//{
						//	mass += body.mass_extra;
						//}

						//data.recoil_multiplier -= Maths.Clamp(MathF.Abs((1.00f / MathF.Max(data.recoil_multiplier * value.Y, 0.10f)) * value.Y), data.recoil_multiplier * 0.20f, data.recoil_multiplier * 2.50f) * value.X * 0.80f; // MathF.Max(data.recoil_multiplier * 0.20f, 0.10f);

						var pellet_count = 0.00f;
						var smirglum_count = 0.00f;

						if (data.ammo_filter.HasAny(Material.Flags.Ammo_LC))
						{
							pellet_count = (1.00f + type) * 0.500f;
							smirglum_count = (1.00f + type) * 0.75f;
						}
						else if (data.ammo_filter.HasAny(Material.Flags.Ammo_HC | Material.Flags.Ammo_SG | Material.Flags.Ammo_Musket))
						{
							pellet_count = (1.00f + type) * 2.00f;
							smirglum_count = (1.00f + type) * 1.50f;
						}
						else if (data.ammo_filter.HasAny(Material.Flags.Ammo_MG | Material.Flags.Ammo_Rocket))
						{
							pellet_count = (1.00f + type) * 8.00f;
							smirglum_count = (1.00f + type) * 4.00f;
						}
						else if (data.ammo_filter.HasAny(Material.Flags.Ammo_Shell | Material.Flags.Ammo_AC))
						{
							pellet_count = (1.00f + type) * 15.00f;
							smirglum_count = (1.00f + type) * 14.00f;
						}

						context.requirements_new.Add(Crafting.Requirement.Resource("pellet.motion", pellet_count), ref mass_added);
						context.requirements_new.Add(Crafting.Requirement.Resource("smirglum.ingot", smirglum_count), ref mass_added);
						context.requirements_new.Add(Crafting.Requirement.Resource("phlogiston", Maths.SnapCeil(smirglum_count * 0.25f, 0.50f)));
						context.requirements_new.Add(Crafting.Requirement.Work(Work.Type.Assembling, 150.00f * smirglum_count * (type + pellet_count) * 0.25f, (byte)(10 + ((type + pellet_count).Pow2()))));

						context.mass_new += mass_added;

						var force = essence_data.force_emit * pellet_count;
						var dist_mult = 1.00f / (1.00f + (Vector2.DistanceSquared(offset, new Vector2(data.receiver_offset.X + 0.125f, data.receiver_offset.Y + 0.125f)) * 10.00f));

						ref var arcane_accelerator = ref context.GetOrAddComponent<ArcaneAccelerator.Data>();
						if (arcane_accelerator.IsNotNull())
						{
							arcane_accelerator.offset = offset;
						}

						ref var light = ref context.GetOrAddTrait<ArcaneAccelerator.Data, Light.Data>();
						if (light.IsNotNull())
						{
							light.color = essence_data.color_emit;
							light.offset = offset - new Vector2(0.750f, 0.00f);
							light.scale = new(2, 1);
							light.intensity = 1.500f;
							light.texture = Light.tex_light_box_00;
						}

						data.velocity_multiplier += MathF.Pow(100 * (1 + type), 1.15f);
						data.recoil_multiplier += MathF.Pow((1 + type) * 0.75f, 0.75f);
						data.damage_multiplier += MathF.Pow((1 + type) * 0.75f, 1.50f);

						//data.recoil_multiplier -= Maths.Normalize(force * dist_mult, context.mass_new * 20.00f);
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.shield",
				category: "Gun (Barrel)",
				name: "Add: Shield",
				description: "Adds a ballistic shield to the gun, allowing it to block incoming projectiles. However, this also makes the weapon much more cumbersome.",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					offset.X.Clamp(data.receiver_offset.X + 0.500f, data.muzzle_offset.X - 0.375f);
					offset.Y.Clamp(data.muzzle_offset.Y - 0.125f, data.muzzle_offset.Y + 0.125f);

					return true;
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle) && !context.HasComponent<Shield.Data>();
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;
					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 3, size: GUI.GetRemainingSpace(x: -GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset, min: new Vector2(data.receiver_offset.X + 0.500f, data.muzzle_offset.Y - 0.125f), max: new Vector2(data.muzzle_offset.X - 0.375f, data.muzzle_offset.Y + 0.125f));

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 3, modifier);

					var frame_y = 0u;

					var sprite = new Sprite("augment.shield", 16, 24, (uint)type, frame_y);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), scale: new(1.00f, 1.00f), pivot: new(0.50f, 0.50f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 3, modifier);

					//ref var melee = ref context.GetOrAddComponent<Melee.Data>(out var init_melee);
					//if (!melee.IsNull())
					{
						//if (init_melee)
						//{
						//	melee.attack_type = Melee.AttackType.Thrust;
						//	melee.aoe = 0.375f;
						//	melee.thickness = 0.375f;
						//	melee.category = Melee.Category.Blunt;
						//	melee.cooldown = 1.00f;
						//	melee.damage_base = 125.00f;
						//	melee.damage_bonus = 200.00f;
						//	melee.damage_type = Damage.Type.Blunt;
						//	melee.knockback = 275.00f;
						//	melee.max_distance = 1.40f;
						//	melee.sound_swing = "tool_swing_00";
						//	melee.hit_mask = Physics.Layer.Destructible | Physics.Layer.Item;
						//	melee.hit_exclude = Physics.Layer.Ignore_Melee | Physics.Layer.Decoration | Physics.Layer.Tree | Physics.Layer.World | Physics.Layer.Building | Physics.Layer.Resource;
						//	melee.flags |= Melee.Flags.Use_RMB;

						//	ref var melee_state = ref context.GetOrAddComponent<Melee.State>();
						//	if (!melee_state.IsNull())
						//	{

						//	}
						//}

						ref var shield = ref context.GetOrAddComponent<Shield.Data>();
						if (shield.IsNotNull())
						{
							ref var armor = ref context.GetOrAddComponent<Armor.Data>();
							if (!armor.IsNull())
							{
								if (armor.material_type == Material.Type.None)
								{
									armor.material_type = Material.Type.Metal;
									armor.toughness = 400.00f;
									armor.protection = 0.90f;
								}

								armor.knockback_modifier += 1.50f;
							}

							ref var cover = ref context.GetOrAddComponent<Cover.Data>();
							if (cover.IsNotNull())
							{
								ref var shape = ref context.GetOrAddTrait<Cover.Data, Shape.Line>();
								if (shape.IsNotNull())
								{
									shape.material = Material.Type.Metal;
									shape.layer |= Physics.Layer.Solid | Physics.Layer.Shield | Physics.Layer.Item;

									switch (type)
									{
										default:
										case 0:
										{
											cover.threshold = 0.55f;

											shape.mass = 2.50f;
											shape.elasticity = 0.75f;
											shape.friction = 0.40f;
											shape.radius = 0.25f;
											shape.a = new Vector2(offset.X, offset.Y - 0.350f);
											shape.b = new Vector2(offset.X, offset.Y + 0.350f);

											//if (init_melee)
											//{
											//	melee.hit_offset = new Vector2(0.000f, 0.000f);
											//	melee.max_distance = 1.000f;
											//	melee.aoe = 0.500f;
											//	melee.thickness = 0.625f;
											//	melee.knockback = 150.00f;
											//	melee.cooldown = 0.750f;
											//	melee.penetration = 0;
											//	melee.penetration_falloff = 0.50f;
											//}

											data.jitter_multiplier += 0.09f;
											data.reload_interval += 0.02f;
										}
										break;

										case 1:
										{
											cover.threshold = 0.30f;

											shape.mass = 4.50f;
											shape.elasticity = 0.75f;
											shape.friction = 0.60f;
											shape.radius = 0.25f;
											shape.a = new Vector2(offset.X, offset.Y - 0.550f);
											shape.b = new Vector2(offset.X, offset.Y + 0.425f);

											//if (init_melee)
											//{
											//	melee.hit_offset = new Vector2(0.000f, 0.000f);
											//	melee.max_distance = 0.750f;
											//	melee.aoe = 0.750f;
											//	melee.thickness = 0.875f;
											//	melee.knockback = 250.00f;
											//	melee.cooldown = 1.00f;
											//	melee.penetration = 2;
											//	melee.penetration_falloff = 0.500f;
											//}

											data.jitter_multiplier += 2.15f;
											data.reload_interval += 0.11f;
										}
										break;

										case 2:
										{
											cover.threshold = 0.05f;

											shape.mass = 13.00f;
											shape.elasticity = 0.45f;
											shape.friction = 0.50f;
											shape.radius = 0.375f;
											shape.a = new Vector2(offset.X, offset.Y - 0.675f);
											shape.b = new Vector2(offset.X, offset.Y + 0.550f);

											//if (init_melee)
											//{
											//	melee.hit_offset = new Vector2(0.000f, 0.000f);
											//	melee.max_distance = 1.250f;
											//	melee.aoe = 0.750f;
											//	melee.thickness = 0.875f;
											//	melee.knockback = 320.00f;
											//	melee.cooldown = 1.50f;
											//	melee.penetration = 2;
											//	melee.penetration_falloff = 0.650f;
											//}

											data.jitter_multiplier += 3.75f;
											data.reload_interval += 0.18f;
										}
										break;

										case 3:
										{
											cover.threshold = 0.25f;

											shape.mass = 6.50f;
											shape.elasticity = 0.85f;
											shape.friction = 0.40f;
											shape.radius = 0.40f;
											shape.a = new Vector2(offset.X - 0.375f, offset.Y - 0.675f);
											shape.b = new Vector2(offset.X, offset.Y + 0.300f);

											//if (init_melee)
											//{
											//	melee.hit_offset = new Vector2(0.000f, 0.000f);
											//	melee.max_distance = 1.250f;
											//	melee.aoe = 0.750f;
											//	melee.thickness = 0.750f;
											//	melee.knockback = 270.00f;
											//	melee.cooldown = 1.30f;
											//	melee.penetration = 1;
											//	melee.penetration_falloff = 0.650f;
											//}

											data.jitter_multiplier += 1.90f;
											data.reload_interval += 0.09f;
										}
										break;
									}
								}
							}
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.arcer",
				category: "Gun (Barrel)",
				name: "Add: Arcer",
				description: "Adds an Arcer to the gun - very similar to the ones seen on Arc Lances.",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					offset.X.Clamp(data.muzzle_offset.X - 0.375f, data.muzzle_offset.X + 0.125f);
					offset.Y.Clamp(data.muzzle_offset.Y - 0.125f, data.muzzle_offset.Y + 0.250f);

					return true;
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle) && !context.HasComponent<ArcLance.Data>() && !context.HasComponent<Melee.Data>();
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;
					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 3, size: GUI.GetRemainingSpace(x: -GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset,
						min: new Vector2(data.muzzle_offset.X - 0.375f, data.muzzle_offset.Y - 0.125f),
						max: new Vector2(data.muzzle_offset.X + 0.125f, data.muzzle_offset.Y + 0.250f));

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 3, modifier);

					var frame_y = 0u;

					var sprite = new Sprite("augment.arcer", 16, 16, (uint)type, frame_y);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), scale: new(1.00f, 1.00f), pivot: new(0.50f, 0.50f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 3, modifier);

					ref var melee = ref context.GetOrAddComponent<Melee.Data>();
					if (!melee.IsNull())
					{
						melee.attack_type = Melee.AttackType.Thrust;
						melee.aoe = 0.500f;
						melee.thickness = 0.375f;
						melee.category = Melee.Category.Pointed;
						melee.cooldown = 1.30f;
						melee.damage_base = 75.00f;
						melee.damage_bonus = 200.00f;
						melee.damage_type = Damage.Type.Stab;
						melee.knockback = 175.00f;
						melee.max_distance = 1.75f;
						melee.sound_swing = "tool_swing_00";
						melee.hit_mask = Physics.Layer.World | Physics.Layer.Destructible;
						melee.hit_require = Physics.Layer.Solid;
						melee.hit_exclude = Physics.Layer.Ignore_Melee | Physics.Layer.Decoration | Physics.Layer.Tree | Physics.Layer.Resource;
						melee.flags |= Melee.Flags.Use_RMB | Melee.Flags.Sync_Hit_Event | Melee.Flags.No_Material_Filter;
						melee.hit_offset = offset;

						ref var melee_state = ref context.GetOrAddComponent<Melee.State>();
						if (!melee_state.IsNull())
						{

						}

						ref var arc_lance = ref context.GetOrAddComponent<ArcLance.Data>();
						if (!arc_lance.IsNull())
						{
							arc_lance.damage_integrity = 500.00f;
							arc_lance.damage_durability = 1500.00f;
							arc_lance.damage_terrain = 150.00f;
						}
					}
				}
			));

			definitions.Add(Augment.Definition.New<Gun.Data>
			(
				identifier: "gun.grip",
				category: "Gun (Barrel)",
				name: "Add: Grip",
				description: "TODO: Desc",

				validate: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					offset.X.Clamp(data.receiver_offset.X + 0.500f, data.muzzle_offset.X + 0.125f);
					offset.Y.Clamp(data.muzzle_offset.Y - 0.000f, data.muzzle_offset.Y + 0.500f);

					return true;
				},

				can_add: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					return !augments.HasAugment(handle);
				},

#if CLIENT
				draw_editor: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();

					var dirty = false;
					dirty |= GUI.SliderIntLerp("Type", ref modifier, 0, 15, size: GUI.GetRemainingSpace(x: -GUI.RmY));
					GUI.SameLine();
					dirty |= GUI.Picker("offset", "Offset", size: new Vector2(GUI.RmY), ref offset,
						min: new Vector2(data.receiver_offset.X + 0.500f, data.muzzle_offset.Y - 0.000f),
						max: new Vector2(data.muzzle_offset.X + 0.125f, data.muzzle_offset.Y + 0.500f));

					return dirty;
				},

				generate_sprite: static (ref Augment.Context context, in Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments, ref DynamicTexture.Context draw) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var frame_y = 0u;

					var sprite = new Sprite("augment.grip", 8, 16, (uint)type, frame_y);

					draw.DrawSprite(sprite, Maths.Snap(offset, 0.125f), scale: new(1.00f, 1.00f), pivot: new(0.50f, 0.50f));
				},
#endif

				apply_0: static (ref Augment.Context context, ref Gun.Data data, ref Augment.Handle handle, Span<Augment.Handle> augments) =>
				{
					ref var offset = ref handle.GetData<Vector2>();
					ref var modifier = ref handle.GetModifier();
					var type = Maths.LerpInt(0, 15, modifier);

					var robustness = 1.00f;
					var size = 1.00f;
					var slant = 1.00f;
					var bulkiness = 1.00f;
					var grip = 1.00f;

					switch (type)
					{
						case 0:
						{
							robustness *= 1.20f;
							size *= 0.70f;
							slant *= 1.35f;
							bulkiness *= 0.80f;
							grip *= 1.10f;
						}
						break;

						case 1:
						{
							robustness *= 0.80f;
							size *= 1.50f;
							slant *= 1.95f;
							bulkiness *= 1.50f;
							grip *= 0.80f;
						}
						break;

						case 2:
						{
							robustness *= 1.50f;
							size *= 1.10f;
							slant *= 0.20f;
							bulkiness *= 1.10f;
							grip *= 0.50f;
						}
						break;

						case 3:
						{
							robustness *= 0.90f;
							size *= 1.40f;
							slant *= 2.10f;
							bulkiness *= 1.10f;
							grip *= 2.20f;
						}
						break;

						case 4:
						{
							robustness *= 1.40f;
							size *= 1.10f;
							slant *= 0.70f;
							bulkiness *= 1.30f;
							grip *= 0.80f;
						}
						break;

						case 5:
						{
							robustness *= 1.10f;
							size *= 1.40f;
							slant *= 0.30f;
							bulkiness *= 0.90f;
							grip *= 5.00f;
						}
						break;

						case 6:
						{
							robustness *= 1.80f;
							size *= 1.40f;
							slant *= 0.20f;
							bulkiness *= 1.50f;
							grip *= 0.70f;
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

					ref var holdable = ref context.GetComponent<Holdable.Data>();
					if (holdable.IsNotNull())
					{
						var mult_muzzle = Maths.InvLerp01(data.receiver_offset.X + 0.500f, data.muzzle_offset.X + 0.125f, offset.X);
						data.jitter_multiplier *= MathF.Max(Maths.Mulpo(mult_muzzle * grip, -0.20f), 0.25f);

						holdable.torque_multiplier *= Maths.Mulpo(mult_muzzle, 0.20f * (grip - (bulkiness * size * 0.40f)));
						holdable.offset.X = Maths.Lerp(Maths.Lerp(context.rect.a.X, holdable.offset.X, 0.50f), offset.X, 0.50f);
						holdable.offset.Y = Maths.Lerp(holdable.offset.Y, offset.Y, 0.80f);
						//if (offset.X <= data.receiver_offset.X + 0.25f) holdable.force_multiplier *= 1.00f + (Maths.MidBias(-0.25f, 0.125f, 0.625f, offset.Y - data.receiver_offset.Y) * Maths.Mulpo(bulkiness, -0.50f) * size * 0.40f);
					}
				}
			));
		}
	}
}

