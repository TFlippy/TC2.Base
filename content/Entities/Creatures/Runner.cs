
namespace TC2.Base.Components
{
	public static partial class Runner
	{
		[IComponent.Data(Net.SendType.Unreliable, region_only: true), IComponent.With<Runner.State>]
		public partial struct Data(): IComponent, IOverridable
		{
			public static bool draw_debug = false;

			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Jump_Align_Surface = 1 << 0,
				Jump_Relative = 1 << 1,
				Move_Relative = 1 << 2,
				Stick_To_Surface = 1 << 3,
				No_Rotate_Align_Surface = 1 << 4,
			}

			public float walk_force;
			public float jump_force;
			public float max_speed = 10.00f;
			public float max_jump_speed = 10.00f;
			public float max_jump_time = 0.50f;
			public float max_air_time = 1.00f;
			public float max_grounded_dot = 0.90f;
			public float propagate_ratio = 1.00f;
			public float no_rotate_air_mult;
			public Runner.Data.Flags flags;

			//public float no_rotate_mult = 1.00f;

			public float walk_lerp = 0.15f;
			public float jump_decay = 0.50f;
			public float crouch_speed_modifier = 0.50f;
			public float air_brake_modifier = 0.10f;
			private float unused_00;
			private float unused_01;
		}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct State(): IComponent
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0,

				Grounded = 1 << 0,
				Walking = 1 << 1,
				Jumping = 1 << 2,
				Falling = 1 << 3,
				Crouching = 1 << 4,
				Climbing = 1 << 5,
				Sitting = 1 << 6,
				WallClimbing = 1 << 7,
				Swimming = 1 << 8,
				//Ragdolled = 1 << 8,
			}

			[Save.Ignore] public float air_modifier_current;
			[Save.Ignore] public float walk_modifier_current;
			[Save.Ignore] public float uphill_force_current;
			[Save.Ignore] public float jump_force_current;
			[Save.Ignore] public float surface_dot_current;
			[Save.Ignore] public float modifier;
			[Save.Ignore] public Runner.State.Flags flags;
			[Save.Ignore] public uint unused_00;

			[Save.Ignore, Net.Ignore] public float air_time;

			[Save.Ignore, Net.Ignore] public float last_jump;
			[Save.Ignore, Net.Ignore] public float last_ground;
			[Save.Ignore, Net.Ignore] public float last_climb;
			[Save.Ignore, Net.Ignore] public float last_air;
			[Save.Ignore, Net.Ignore] public float last_move;
			[Save.Ignore, Net.Ignore] public float last_swim;
			[Save.Ignore, Net.Ignore] private float unused_01;

			[Save.Ignore, Net.Ignore] public Vector2 last_normal;
			[Save.Ignore, Net.Ignore] public Vector2 last_force;
			[Save.Ignore, Net.Ignore] public Vector2 last_wallclimb_force;
			[Save.Ignore, Net.Ignore] private Vector2 unused_02;
		}

		[ISystem.Monitor(ISystem.Mode.Single, ISystem.Scope.Region), HasRelation(Source.Modifier.Any, Relation.Type.Seat, true), HasTag("initialized", true, Source.Modifier.Owned)]
		public static void OnAddRemoveSit(ISystem.Info info, [Source.Owned] ref Runner.State runner_state)
		{
			switch (info.EventType)
			{
				case ISystem.EventType.Add:
				case ISystem.EventType.Set:
				{
					runner_state.flags.AddFlag(Runner.State.Flags.Sitting);
				}
				break;

				case ISystem.EventType.Remove:
				case ISystem.EventType.Unset:
				{
					runner_state.flags.RemoveFlag(Runner.State.Flags.Sitting);
				}
				break;
			}
		}

		//[ISystem.AddFirst(ISystem.Mode.Single, ISystem.Scope.Region), HasRelation(Source.Modifier.Any, Relation.Type.Seat, true), HasTag("initialized", true, Source.Modifier.Owned)]
		//public static void OnSit(ISystem.Info info, Entity entity, [Source.Owned] ref Runner.State runner_state)
		//{
		//	runner_state.flags.AddFlag(Runner.State.Flags.Sitting);
		//}

		//[ISystem.RemoveLast(ISystem.Mode.Single, ISystem.Scope.Region), HasRelation(Source.Modifier.Any, Relation.Type.Seat, true)]
		//public static void OnUnSit(ISystem.Info info, Entity entity, [Source.Owned] ref Runner.State runner_state)
		//{
		//	runner_state.flags.RemoveFlag(Runner.State.Flags.Sitting);
		//}

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateOrganic([Source.Owned, Override] ref Runner.Data runner, 
		[Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state)
		{
			//App.WriteLine($"{organic.dexterity}");

			var stun_mult = (1.00f - organic_state.stun_norm);

			runner.walk_force *= Maths.Clamp(organic.strength * organic.coordination * organic.motorics, 0.50f, 1.80f) * stun_mult;
			runner.jump_force *= Maths.Clamp(Maths.Min(organic.strength, organic.dexterity), 0.75f, 1.40f) * organic.coordination * organic.motorics * stun_mult;
			runner.max_speed *= Maths.Clamp(organic_state.efficiency * organic.coordination, 0.50f, 1.20f) * stun_mult;
			runner.max_jump_speed *= (organic_state.efficiency * 1.50f).Clamp01() * stun_mult;
			//runner.no_rotate_mult *= 

			//if (organic_state.efficiency < 0.50f)
			//{
			//	runner.max_speed *= 0.50f;
			//	runner.max_jump_speed *= 0.20f;
			//}
		}

		[ISystem.Update.A(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateNoRotate(ISystem.Info info,
		[Source.Owned, Override] in Runner.Data runner, [Source.Owned] ref Runner.State runner_state, 
		[Source.Owned, Override] ref NoRotate.Data no_rotate, [Source.Owned] ref Control.Data control)
		{
			//var modifier = 1.00f - (info.WorldTime - Maths.Max(runner_state.last_climb, Maths.Max(runner_state.last_ground, runner_state.last_jump + runner.max_jump_time))).Clamp01();
			var modifier = Maths.Lerp01(1.00f, runner.no_rotate_air_mult, Maths.Normalize(info.WorldTime - Maths.Max(runner_state.last_climb, runner_state.last_ground, runner_state.last_jump + runner.max_jump_time), runner.max_air_time));
			if (control.keyboard.GetKey(Keyboard.Key.X))
			{
				modifier = 0.00f;
				//control.keyboard.SetKeyPressed(Keyboard.Key.NoMove, true);
			}

			//no_rotate.speed *= modifier;
			no_rotate.multiplier *= modifier;
			no_rotate.mass_multiplier *= modifier;

			if (runner.flags.HasAny(Runner.Data.Flags.No_Rotate_Align_Surface)) no_rotate.rotation = -(runner_state.last_normal.GetAngleRadiansFast()) - (MathF.PI * 0.50f);
		}

		[ISystem.Update.B(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateNoRotateParent(ISystem.Info info,
		[Source.Owned, Override] in Runner.Data runner, [Source.Owned] ref Runner.State runner_state, 
		[Source.Parent, Override] ref NoRotate.Data no_rotate_parent, [Source.Owned] ref Control.Data control)
		{
			if (no_rotate_parent.flags.HasAny(NoRotate.Flags.No_Share)) return;

			//var modifier = 1.00f - (info.WorldTime - Maths.Max(runner_state.last_climb, Maths.Max(runner_state.last_ground, runner_state.last_jump + runner.max_jump_time))).Clamp01();
			var modifier = 1.00f;
			if (control.keyboard.GetKey(Keyboard.Key.X))
			{
				modifier = 0.00f;
				//control.keyboard.SetKeyPressed(Keyboard.Key.NoMove, true);
			}
			else
			{
				modifier = Maths.Lerp01(modifier, runner.no_rotate_air_mult, Maths.Normalize(info.WorldTime - Maths.Max(runner_state.last_climb, runner_state.last_swim, runner_state.last_ground, runner_state.last_jump + runner.max_jump_time), runner.max_air_time));
			}

			//no_rotate_parent.speed = Maths.Lerp(no_rotate_parent.speed, no_rotate_parent.speed * modifier, runner.propagate_ratio);

			if (runner_state.flags.HasAny(Runner.State.Flags.Swimming))
			{
				no_rotate_parent.multiplier *= modifier;
				no_rotate_parent.mass_multiplier *= modifier;

				no_rotate_parent.rotation = 0.00f;
				no_rotate_parent.flags.AddFlag(NoRotate.Flags.No_Share);
			}
			else
			{
				no_rotate_parent.multiplier = Maths.Lerp(no_rotate_parent.multiplier, no_rotate_parent.multiplier * modifier, runner.propagate_ratio);
				no_rotate_parent.mass_multiplier = Maths.Lerp(no_rotate_parent.mass_multiplier, no_rotate_parent.mass_multiplier * modifier, runner.propagate_ratio);
			}
		}

		[ISystem.Update.C(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateClimbing(ISystem.Info info, //, Entity ent_runner, Entity ent_climber,
		/*[Source.Owned, Override] in Runner.Data runner,*/ [Source.Owned] ref Runner.State runner_state,
		[Source.Any] in Climber.Data climber) //, [Source.Owned, Override, Optional(true)] ref NoRotate.Data no_rotate)
		{
			runner_state.flags.SetFlag(Runner.State.Flags.Climbing, climber.cling_entity.IsValid());
			runner_state.flags.SetFlag(Runner.State.Flags.WallClimbing, climber.wallclimb_timer >= 0.40f);

			if (runner_state.flags.HasAny(Runner.State.Flags.Climbing))
			{
				runner_state.last_climb = info.WorldTime;
				runner_state.last_wallclimb_force = climber.last_force;

				//if (App.IsClient) info.GetRegion().DrawDebugText(ent_runner.GetTransform().position, $"{ent_runner}; {ent_climber}; {no_rotate.IsNotNull()}", Color32BGRA.White);

				//if (ent_runner != ent_climber && no_rotate.IsNotNull())
				//{

				//	no_rotate.multiplier = 0.00f;
				//	no_rotate.mass_multiplier = 0.00f;
				//}
			}
		}

		//		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		//		public static void UpdateTest(ISystem.Info info, [Source.Owned] ref Body.Data body, [Source.Owned] in Organic.Data organic)
		//		{
		//			ref var region = ref info.GetRegion();

		//#if CLIENT
		//			var color = new Color32BGRA(0xff00ff00);
		//#else
		//			var color = new Color32BGRA(0xffff0000);
		//#endif

		//			region.DrawDebugBody(ref body, color.WithAlphaMult(0.50f));
		//		}

		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateMovement(ISystem.Info info, ref Region.Data region, [Source.Owned] in Transform.Data transform, 
		[Source.Owned] ref Body.Data body, [Source.Owned] in Control.Data control,
		[Source.Owned, Override] in Runner.Data runner, [Source.Owned] ref Runner.State runner_state,
		[Source.Owned] in Physics.Data physics, [Source.Owned, Pair.Component<Physics.Data>, Optional] in Net.Synchronized synchronized)
		{
			var kb = control.keyboard;

			var has_authority = synchronized.HasAuthority();

			var time = info.WorldTime;
			var force = new Vector2(0, 0);
			var max_speed = new Vector2(runner.max_speed, runner.max_jump_speed);
			var velocity = body.GetVelocity();
			var mass = body.GetMass();

			var can_move = !kb.GetKey(Keyboard.Key.NoMove | Keyboard.Key.X) && !runner_state.flags.HasAny(Runner.State.Flags.Sitting);
			var is_walking = can_move && kb.GetKey(Keyboard.Key.MoveLeft | Keyboard.Key.MoveRight);
			var any = can_move && kb.GetKey(Keyboard.Key.MoveLeft | Keyboard.Key.MoveRight | Keyboard.Key.MoveUp);
			var is_swimming = false;
			var is_climbing = runner_state.flags.HasAny(Runner.State.Flags.Climbing);
			var stick_to_surface = runner.flags.HasAny(Runner.Data.Flags.Stick_To_Surface);
			var move_relative = runner.flags.HasAny(Runner.Data.Flags.Move_Relative);

			var dir_v = velocity.GetNormalized();
			//var dir_l = stick_to_surface ? runner_state.last_normal.RotateByRad(-MathF.PI * 0.50f) : move_relative ? transform.Left : new(-1.00f, 0.00f);
			//var dir_r = stick_to_surface ? runner_state.last_normal.RotateByRad(MathF.PI * 0.50f) : move_relative ? transform.Right : new(1.00f, 0.00f);
			var dir_l = stick_to_surface ? runner_state.last_normal.GetPerpendicular(false) : move_relative ? transform.Left : new(-1.00f, 0.00f);
			var dir_r = stick_to_surface ? runner_state.last_normal.GetPerpendicular(true) : move_relative ? transform.Right : new(1.00f, 0.00f);


			//if (any || kb.GetKeyDown(Keyboard.Key.NoMove | Keyboard.Key.X | Keyboard.Key.MoveDown) || kb.GetKeyUp(Keyboard.Key.NoMove | Keyboard.Key.X | Keyboard.Key.MoveDown))
			if (any || kb.GetKeyChanged(Keyboard.Key.NoMove | Keyboard.Key.X | Keyboard.Key.MoveDown))
			{
				body.Activate();
			}

			runner_state.flags.SetFlag(Runner.State.Flags.Walking, is_walking);

#if CLIENT
			//region.DrawDebugDir(body.GetPosition(), runner_state.last_normal.RotateByRad(-MathF.PI * 0.50f) * 4, Color32BGRA.FromHSV(0, 1, 1).WithAlphaMult(0.50f));
			//region.DrawDebugDir(body.GetPosition(), runner_state.last_normal.RotateByRad(MathF.PI * 0.50f) * 4, Color32BGRA.FromHSV(0, 1, 1).WithAlphaMult(0.50f));
			//region.DrawDebugDir(body.GetPosition(), runner_state.last_normal.GetPerpendicular(false) * 4, Color32BGRA.FromHSV(2, 1, 1).WithAlphaMult(0.50f));
			//region.DrawDebugDir(body.GetPosition(), runner_state.last_normal.GetPerpendicular(true) * 4, Color32BGRA.FromHSV(2, 1, 1).WithAlphaMult(0.50f));
#endif

			if (is_walking)
			{
				runner_state.last_move = time;
				runner_state.walk_modifier_current = Maths.Lerp(runner_state.walk_modifier_current, 1.00f, runner.walk_lerp);

				if (kb.GetKey(Keyboard.Key.MoveLeft) && Vector2.Dot(dir_v, dir_l) < runner.max_speed) force += dir_l * runner.walk_force * runner_state.walk_modifier_current;
				if (kb.GetKey(Keyboard.Key.MoveRight) && Vector2.Dot(dir_v, dir_r) < runner.max_speed) force += dir_r * runner.walk_force * runner_state.walk_modifier_current;
				//if (velocity.X < +runner.max_speed && kb.GetKey(Keyboard.Key.MoveRight)) force.X += runner.walk_force * runner_state.walk_modifier_current;
			}
			else
			{
				runner_state.walk_modifier_current = 0.00f;
			}

			var layers = default(Physics.Layer);
			var normal = Vector2.Zero;
			var rv = Vector2.Zero;
			var friction = 0.00f;

			var arbiter_count = 0;
			foreach (var arbiter in body.GetArbiters())
			{
				var arbiter_layer = arbiter.GetLayer();
				//App.WriteLine(arbiter.GetRigidityDynamic());
				if (arbiter.GetRigidityDynamic() > 0.90f) // || arbiter.GetLayer().HasAny(Physics.Layer.Water))
				{
					normal += arbiter.GetNormal();
					friction += arbiter.GetFriction();
					layers |= arbiter_layer;
					rv += arbiter.GetBodyVelocity();
					//arbiter.GetVelocity

					arbiter_count++;
				}
				else if (arbiter_layer.HasAny(Physics.Layer.Water))
				{
					is_swimming = true;
				}
			}

			if (arbiter_count > 0)
			{
				normal /= arbiter_count;
				friction /= arbiter_count;
			}

			normal = normal.GetNormalizedFast();
			runner_state.last_normal = Maths.LerpFMA(runner_state.last_normal, normal, 0.20f).GetNormalizedFast();

			var is_grounded = false;
			var is_on_vehicle = false;

			if (is_swimming)
			{
				runner_state.last_swim = time;
			}

			if (arbiter_count > 0) // && !layers.HasAll(Physics.Layer.Bounds))
			{
				//is_grounded = MathF.Abs(normal.X) < 0.35f;


				var dot = MathF.Abs(runner_state.last_normal.X);
				is_grounded = dot <= runner.max_grounded_dot;
				is_on_vehicle = dot <= 0.75f && layers.HasAny(Physics.Layer.Vehicle | Physics.Layer.Platform);
				runner_state.surface_dot_current = dot;

				//runner_state.uphill_force_current = -MathF.Abs(dot * force.Length()) * friction * 0.50f;

				if (stick_to_surface)
				{

				}
				else
				{
					runner_state.uphill_force_current = -MathF.Abs(dot * force.X);
					force.Y = runner_state.uphill_force_current;
					force.X *= (1.00f - dot);
				}
			}
			else
			{
				if (stick_to_surface)
				{
					force *= 0.00f;
				}
				else if (!is_climbing)
				{
					force.X *= 0.75f;
					force.Y *= 0.00f;

					force.Y -= runner_state.uphill_force_current * 0.75f;
					runner_state.uphill_force_current *= 0.50f;
					runner_state.surface_dot_current = 0.00f;
				}
			}

			//#if CLIENT
			//			region.DrawDebugText(body.GetPosition(), $"{arbiter_count}\n{runner_state.flags}\n{is_on_vehicle}", Color32BGRA.Yellow);
			//#endif

			//#if CLIENT
			//			region.DrawNormal(body.GetPosition(), runner_state.last_normal, is_grounded ? Color32BGRA.Green : Color32BGRA.Yellow);
			//#endif

			if (!stick_to_surface && runner_state.flags.HasAny(Runner.State.Flags.WallClimbing))
			{
				force.X *= 0.70f;
				force.Y *= 0.20f;
				//runner_state.uphill_force_current *= 0.00f;
			}

			var jump_dir = new Vector2(0, -1);

			if (is_grounded)
			{
				runner_state.flags.AddFlag(Runner.State.Flags.Grounded);
				runner_state.last_ground = time;

				if (runner.flags.HasAny(Runner.Data.Flags.Jump_Align_Surface)) jump_dir = normal;
			}
			else
			{
				runner_state.flags.RemoveFlag(Runner.State.Flags.Grounded);
				if (!is_swimming)
				{
					runner_state.last_air = time;
					if (runner.flags.HasAny(Runner.Data.Flags.Jump_Relative)) jump_dir = transform.Up;
				}
			}

			//if (can_move && keyboard.GetKey(Keyboard.Key.MoveUp) && (time - runner_state.last_jump) > 0.40f && (time - runner_state.last_ground) < 0.20f && !runner_state.flags.HasAny(Runner.State.Flags.WallClimbing))
			if (can_move && kb.GetKey(Keyboard.Key.MoveUp) && (time - runner_state.last_jump) > 0.40f && Maths.Min(time - runner_state.last_ground, time - runner_state.last_climb) < 0.20f && runner_state.flags.HasNone(Runner.State.Flags.WallClimbing | Runner.State.Flags.Climbing))
			{
				runner_state.jump_force_current = runner.jump_force;
				runner_state.last_jump = time;
			}

			//force.Y -= runner_state.jump_force_current;

			force += jump_dir * runner_state.jump_force_current;
			//force -= runner_state.jump_force_current;
			runner_state.jump_force_current *= runner.jump_decay;

			runner_state.flags.SetFlag(Runner.State.Flags.Crouching, can_move && control.keyboard.GetKey(Keyboard.Key.MoveDown) && (time - runner_state.last_ground) <= 0.25f);
			if (runner_state.flags.HasAny(Runner.State.Flags.Crouching))
			{
				runner_state.jump_force_current *= 0.50f;
				max_speed.X *= runner.crouch_speed_modifier;
			}

			if (!stick_to_surface && !is_walking && can_move && (velocity - rv).LengthSquared() > 0.10f) // && !is_on_vehicle && (time - runner_state.last_move) <= 0.50f)
			{
				var required_force_dir = ((velocity - rv) * mass * App.tickrate) - force;
				required_force_dir = required_force_dir.GetNormalized(out var required_force_magnitude);
				required_force_dir *= Maths.Clamp(runner.walk_force * 0.50f, -required_force_magnitude, required_force_magnitude);
				required_force_dir.Y = 0.00f;

				if (runner_state.flags.HasNone(Runner.State.Flags.Grounded))
				{
					required_force_dir.X *= runner.air_brake_modifier;
				}

				force -= required_force_dir * Maths.Max(1.00f - (time - runner_state.last_move), 0.00f);
			}

			//max_speed *= runner_state.speed_modifier;
			//force *= runner_state.force_modifier;

			runner_state.air_time = time - Maths.Max(runner_state.last_climb, runner_state.last_ground, runner_state.last_jump, runner_state.last_swim);
			runner_state.air_modifier_current = Maths.Lerp(runner_state.air_modifier_current, 1.00f - Maths.Clamp(runner_state.air_time - 0.75f, 0.00f, 1.00f), 0.10f);
			if (!is_swimming) force *= runner_state.air_modifier_current;

			//#if SERVER
			//			WorldNotification.Push(ref region, $"{runner.max_speed:0.00}", Color32BGRA.Green, body.GetPosition(), lifetime: 0.30f);
			//#endif

			force = Physics.LimitForce2B(ref body, force, max_speed);

			runner_state.flags.SetFlag(State.Flags.Falling, !is_swimming && velocity.Y > 0.10f && runner_state.air_time > 0.040f);
			runner_state.flags.SetFlag(State.Flags.Jumping, runner_state.jump_force_current > 1.00f);
			runner_state.flags.SetFlag(State.Flags.Swimming, is_swimming);

#if CLIENT
			if (Runner.Data.draw_debug)
			{
				region.DrawDebugDir(transform.position, transform.Right, Color32BGRA.FromHSV(0, 1, 1).WithAlphaMult(0.50f));
				region.DrawDebugDir(transform.position, transform.Up, Color32BGRA.FromHSV(1, 1, 1).WithAlphaMult(0.50f));
				region.DrawDebugDir(transform.position, transform.Left, Color32BGRA.FromHSV(2, 1, 1).WithAlphaMult(0.50f));
				region.DrawDebugDir(transform.position, transform.Down, Color32BGRA.FromHSV(3, 1, 1).WithAlphaMult(0.50f));

				region.DrawDebugDir(transform.position, dir_v, Color32BGRA.FromHSV(4, 1, 1), thickness: 3.00f);
				region.DrawDebugDir(transform.position, force.GetNormalized() * 1.50f, Color32BGRA.FromHSV(5, 1, 1), thickness: 2.00f);
			}
#endif


			runner_state.last_force = force;
			//if ((has_authority || physics.position.IsInDistance(body.GetPosition(), 2.00f) && force.LengthSquared() > 0.10f))
			if ((force.LengthSquared() > 0.10f) && (has_authority || (physics.elapsed < 1.00f && (physics.position + (physics.velocity * physics.elapsed)).IsInDistance(body.GetPosition(), 3.00f))))
			{
				body.AddForce(force);
			}

			if (stick_to_surface && (time - runner_state.last_ground <= 0.40f) && !kb.GetKeyDown(Keyboard.Key.MoveUp)) body.AddForce(-runner_state.last_normal * body.GetMass() * region.GetGravity().Length() * 1.50f * Maths.Normalize01(time - runner_state.last_ground, 0.40f).Inv().Pow2());

			//#if CLIENT
			//			region.DrawText(body.GetPosition(), $"{runner_state.flags}\n{runner_state.last_jump:0.00}\n{(info.WorldTime - runner_state.last_ground):0.00}", Color32BGRA.White);
			//#endif
		}
	}
}
