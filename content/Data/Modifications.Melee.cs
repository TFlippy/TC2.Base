using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		private static void RegisterMeleeModifications(ref List<Modification.Definition> definitions)
		{
			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.battering",
				name: "Battering",
				description: "Greatly increases knockback.",

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (data.knockback == 0.00f)
					{
						data.knockback += 0.20f;
					}

					switch (data.category)
					{
						case Melee.Category.Pointed:
						{
							data.knockback *= 1.35f;
							data.damage_base *= 1.10f;
							data.damage_bonus *= 0.90f;
						}
						break;

						case Melee.Category.Bladed:
						{
							data.knockback *= 1.20f;
							data.damage_base *= 0.80f;
							data.damage_bonus *= 0.90f;
						}
						break;

						case Melee.Category.Blunt:
						{
							data.knockback *= 1.75f;
							data.damage_base *= 1.10f;
							data.damage_bonus *= 1.40f;
						}
						break;
					}

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 1.30f;
					}
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							requirement.amount *= 1.30f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.10f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.hooking",
				name: "Hooking",
				description: "Inverts the knockback by adding tiny hooks.",

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return !modifications.HasModification(handle);
				},

				apply_0: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.knockback *= -0.75f;
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							requirement.amount *= 1.10f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.30f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.longer_handle",
				name: "Longer Handle",
				description: "Lengthens the handle of the weapon, allowing you to strike further with more power.",

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (data.flags.HasAny(Melee.Flags.No_Handle)) return false;

					var count = 0;
					for (int i = 0; i < modifications.Length; i++)
					{
						if (modifications[i].id == handle.id) count++;
					}
					return count < 2;
				},

				apply_0: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.max_distance *= 1.20f;
					data.cooldown *= 1.10f;
					data.damage_base *= 1.15f;
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							requirement.amount *= 1.10f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.uneven_strike",
				name: "Uneven Strike",
				description: "Reduces base damage, but also increases bonus damage.",

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var count = 0;
					for (int i = 0; i < modifications.Length; i++)
					{
						if (modifications[i].id == handle.id) count++;
					}
					return count < 2;
				},

				apply_0: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.damage_bonus += data.damage_base * 0.50f;
					data.damage_base *= 0.80f;
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							requirement.amount *= 1.20f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 0.80f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.reliable_strike",
				name: "Reliable Strike",
				description: "Half of the weapon's bonus damage is converted to base damage",

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					var count = 0;
					for (int i = 0; i < modifications.Length; i++)
					{
						if (modifications[i].id == handle.id) count++;
					}
					return count < 2 && data.damage_bonus > 0.00f;
				},

				apply_0: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.damage_base += data.damage_bonus * 0.50f;
					data.damage_bonus *= 0.50f;
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							//requirement.amount *= 1.10f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.20f;
							requirement.difficulty += 1.00f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.hollowed",
				name: "Hollowed",
				description: "Hollows out parts of the weapon, increasing swing speed, while also reducing damage, knockback and cost.",

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					if (data.category != Melee.Category.Blunt) return false;

					var count = 0;
					for (int i = 0; i < modifications.Length; i++)
					{
						if (modifications[i].id == handle.id) count++;
					}
					return count < 2;
				},

				apply_0: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.damage_base *= 0.80f;
					data.damage_bonus *= 0.90f;
					data.knockback *= 0.30f;
					data.cooldown *= 0.75f;
					// TODO: This should reduce the weight as well

					ref var body = ref context.GetComponent<Body.Data>();
					if (!body.IsNull())
					{
						body.mass_multiplier *= 0.70f;
					}
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Resource)
						{
							requirement.amount *= 0.70f;
						}
						else if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.70f;
							requirement.difficulty += 1.00f;
							if (requirement.work == Work.Type.Smithing)
							{
								requirement.difficulty += 3.00f;
								requirement.amount += 100.00f;
							}
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.resourceful_sharpening",
				name: "Resourceful Sharpening",
				description: "Sharpens the tool to render less of the struck material unusable, increasing resource yield.",

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return (data.category == Melee.Category.Bladed || data.category == Melee.Category.Pointed) && data.yield > 0.00f && data.yield < 1.50f;
				},

				apply_0: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.damage_base *= 0.90f;
					data.damage_bonus *= 0.90f;
					data.yield = Maths.Clamp(data.yield + 0.10f, 0.00f, 1.00f);
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 1.20f;
							requirement.difficulty += 3.00f;
						}
					}
				}
			));

			definitions.Add(Modification.Definition.New<Melee.Data>
			(
				identifier: "melee.toy_weapon",
				name: "Toy Weapon",
				description: "Dulls the weapon to deal no damage.", // Currently this also reduces kockback to 0 cause knockback is bad and based on damage

				can_add: static (ref Modification.Context context, in Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					return data.damage_base > 0.00f || data.damage_bonus > 0.00f;
				},

				finalize: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					data.damage_base = 0.00f;
					data.damage_bonus = 0.00f;

					return true;
				},

				apply_1: static (ref Modification.Context context, ref Melee.Data data, ref Modification.Handle handle, Span<Modification.Handle> modifications) =>
				{
					foreach (ref var requirement in context.requirements_new)
					{
						if (requirement.type == Crafting.Requirement.Type.Work)
						{
							requirement.amount *= 0.70f;
						}
					}
				}
			));
		}
	}
}

