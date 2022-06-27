using TC2.Base.Components;

namespace TC2.Base
{
	public sealed partial class ModInstance
	{
		private static void RegisterControlAugments(ref List<Augment.Definition> definitions)
		{
			definitions.Add(Augment.Definition.New<Control.Data>
			(
				identifier: "control.random_activation",
				category: "Control",
				name: "Random Activation",
				description: "Item randomly activates on its own.",

				// This randomly causes a left click and a space bar (at the same time)
				// Working examples: guns, fuse explosives, drills, mounts (yes they will use whatever is on them), melee weapons, even medkits
				// Due to the wide variety of uses this has, this costs a large amount of materials
				// This doesn't aim, so using anything which uses aim direction requires additional setup

				apply_0: static (ref Augment.Context context, ref Control.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					ref var random_activation = ref context.GetOrAddComponent<RandomActivation.Data>();
					random_activation.duration += 0.20f;
				},

				apply_1: static (ref Augment.Context context, ref Control.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("pellet.motion", 10.00f)); // High cost

					if (!context.GetComponent<Melee.Data>().IsNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource("pellet.motion", 10.00f)); // Even higher cost on melee weapons
					}
				}
			));
			definitions.Add(Augment.Definition.New<Control.Data>
			(
				identifier: "control.mind_swap",
				category: "Control",
				name: "Mind Swap",
				description: "Swap your mind with this when holding it.",

				//Once held by a character they are mind swapped with this thing
				//Also adds some movementspeed so you can walk around and aren't stuck
				//Also becomes a threat so that animals still attack you while you are an object
				//Very hard to use if you are a gun since A you can't reload since you can't hold ammo and B you can only aim by flinging yourself around

				can_add: static (ref Augment.Context context, in Control.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					return !Augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Control.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					ref var mindswap = ref context.GetOrAddComponent<Mindswap.Data>();
					ref var runner = ref context.GetOrAddComponent<Runner.Data>();
					ref var runnerstate = ref context.GetOrAddComponent<Runner.State>();
					ref var threat = ref context.GetOrAddComponent<Threat.Data>();
					if (threat.priority == 0) threat.priority = 7.00f;
					if (runner.walk_force == 0) runner.walk_force = 1000.00f;
					if (runner.jump_force == 0) runner.jump_force = 2000.00f;
				},

				apply_1: static (ref Augment.Context context, ref Control.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("pellet.cognition", 15.00f)); // High cost
				}
			));
			definitions.Add(Augment.Definition.New<Control.Data>
			(
				identifier: "control.mind_control",
				category: "Control",
				name: "Mind Control",
				description: "Becomes controlled by the first person holding it.",

				//You can control multiple things this way if you want like turrets

				can_add: static (ref Augment.Context context, in Control.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					return !Augments.HasAugment(handle);
				},

				apply_0: static (ref Augment.Context context, ref Control.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					ref var mindcontrol = ref context.GetOrAddComponent<Mindcontrol.Data>();
					ref var threat = ref context.GetOrAddComponent<Threat.Data>();
					if (threat.priority == 0) threat.priority = 7.00f;
				},

				apply_1: static (ref Augment.Context context, ref Control.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("pellet.cognition", 30.00f)); // Higher cost
				}
			));
			/*definitions.Add(Augment.Definition.New<Control.Data>
			(
				identifier: "control.auto_aim",
				category: "Control",
				name: "Auto Aim",
				description: "Tries to automaticly aim towards nearby other creatures.",
				//APPARENTLY THIS JUST DOESNT WORK

				apply_0: static (ref Augment.Context context, ref Control.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					ref var npc = ref context.GetOrAddComponent<NPC.Data>();
					
					npc.navigation_state = NPC.NavigationState.Searching;
					npc.find_distance_max = 10.00f;
					npc.lose_distance_max = 128.00f;
				},

				apply_1: static (ref Augment.Context context, ref Control.Data data, ref Augment.Handle handle, Span<Augment.Handle> Augments) =>
				{
					context.requirements_new.Add(Crafting.Requirement.Resource("pellet.motion", 10.00f)); // High cost

					if (!context.GetComponent<Melee.Data>().IsNull())
					{
						context.requirements_new.Add(Crafting.Requirement.Resource("pellet.motion", 10.00f)); // Even higher cost on melee weapons
					}
				}
			));*/
		}
	}
}

