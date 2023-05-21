
namespace TC2.Base.Components
{
	public static partial class Runner
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
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent, IOverridable
		{
			public float walk_force;
			public float jump_force;
			public float max_speed = 10.00f;
			public float max_jump_speed = 10.00f;
			public float max_jump_time = 0.50f;
			public float max_air_time = 1.00f;

			public float walk_lerp = 0.15f;
			public float jump_decay = 0.50f;
			public float crouch_speed_modifier = 0.50f;
			public float air_brake_modifier = 0.10f;

			public Data()
			{

			}
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct State: IComponent
		{
			[Save.Ignore] public float air_modifier_current;
			[Save.Ignore] public float walk_modifier_current;
			[Save.Ignore] public float uphill_force_current;
			[Save.Ignore] public float jump_force_current;
			[Save.Ignore] public Runner.Flags flags;

			[Save.Ignore, Net.Ignore] public float air_time;

			[Save.Ignore, Net.Ignore] public float last_jump;
			[Save.Ignore, Net.Ignore] public float last_ground;
			[Save.Ignore, Net.Ignore] public float last_climb;
			[Save.Ignore, Net.Ignore] public float last_air;
			[Save.Ignore, Net.Ignore] public float last_move;

			[Save.Ignore, Net.Ignore] public Vector2 last_normal;
			[Save.Ignore, Net.Ignore] public Vector2 last_force;
			[Save.Ignore, Net.Ignore] public Vector2 last_wallclimb_force;

			public State()
			{

			}
		}

		[ISystem.AddFirst(ISystem.Mode.Single), HasRelation(Source.Modifier.Any, Relation.Type.Seat, true)]
		public static void OnSit(ISystem.Info info, Entity entity, [Source.Owned] ref Runner.State runner_state)
		{
			runner_state.flags.SetFlag(Runner.Flags.Sitting, true);
		}

		[ISystem.RemoveLast(ISystem.Mode.Single), HasRelation(Source.Modifier.Any, Relation.Type.Seat, true)]
		public static void OnUnSit(ISystem.Info info, Entity entity, [Source.Owned] ref Runner.State runner_state)
		{
			runner_state.flags.SetFlag(Runner.Flags.Sitting, false);
		}

		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateOrganic(ISystem.Info info, [Source.Owned, Override] ref Runner.Data runner, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state)
		{
			//App.WriteLine($"{organic.dexterity}");

			runner.walk_force *= Maths.Clamp(organic.strength * organic.coordination * organic.motorics, 0.50f, 1.80f);
			runner.jump_force *= Maths.Clamp(MathF.Min(organic.strength, organic.dexterity), 0.75f, 1.40f) * organic.coordination * organic.motorics;
			runner.max_speed *= Maths.Clamp(organic_state.efficiency * organic.coordination, 0.50f, 1.20f);
			runner.max_jump_speed *= (organic_state.efficiency * 1.50f).Clamp01();

			//if (organic_state.efficiency < 0.50f)
			//{
			//	runner.max_speed *= 0.50f;
			//	runner.max_jump_speed *= 0.20f;
			//}
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateClimbing(ISystem.Info info, [Source.Owned, Override] in Runner.Data runner, [Source.Owned] ref Runner.State runner_state, [Source.Parent] in Climber.Data climber)
		{
			runner_state.flags.SetFlag(Runner.Flags.Climbing, climber.cling_entity.IsValid());
			runner_state.flags.SetFlag(Runner.Flags.WallClimbing, climber.wallclimb_timer >= 0.40f);

			if (runner_state.flags.HasAny(Runner.Flags.Climbing))
			{
				runner_state.last_climb = info.WorldTime;
			}

			if (runner_state.flags.HasAll(Runner.Flags.Climbing))
			{
				runner_state.last_wallclimb_force = climber.last_force;
			}
		}

		[ISystem.Update(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateNoRotate(ISystem.Info info, Entity entity, 
		[Source.Owned, Override] in Runner.Data runner, [Source.Owned] ref Runner.State runner_state, [Source.Owned, Override] ref NoRotate.Data no_rotate, [Source.Owned] ref Control.Data control)
		{
			//var modifier = 1.00f - (info.WorldTime - MathF.Max(runner_state.last_climb, MathF.Max(runner_state.last_ground, runner_state.last_jump + runner.max_jump_time))).Clamp01();
			var modifier = 1.00f - Maths.NormalizeClamp(info.WorldTime - MathF.Max(runner_state.last_climb, MathF.Max(runner_state.last_ground, runner_state.last_jump + runner.max_jump_time)), runner.max_air_time);
			if (control.keyboard.GetKey(Keyboard.Key.X))
			{
				modifier *= 0.00f;
				//control.keyboard.SetKeyPressed(Keyboard.Key.NoMove, true);
			}

			no_rotate.multiplier *= modifier;
			no_rotate.mass_multiplier *= modifier;
		}

		[ISystem.Update(ISystem.Mode.Single), HasTag("dead", false, Source.Modifier.Owned)]
		public static void UpdateNoRotateParent(ISystem.Info info, Entity entity, 
		[Source.Owned, Override] in Runner.Data runner, [Source.Owned] ref Runner.State runner_state, [Source.Parent, Override] ref NoRotate.Data no_rotate_parent, [Source.Owned] ref Control.Data control)
		{
			//var modifier = 1.00f - (info.WorldTime - MathF.Max(runner_state.last_climb, MathF.Max(runner_state.last_ground, runner_state.last_jump + runner.max_jump_time))).Clamp01();
			var modifier = 1.00f - Maths.NormalizeClamp(info.WorldTime - MathF.Max(runner_state.last_climb, MathF.Max(runner_state.last_ground, runner_state.last_jump + runner.max_jump_time)), runner.max_air_time);
			if (control.keyboard.GetKey(Keyboard.Key.X))
			{
				modifier *= 0.00f;
				//control.keyboard.SetKeyPressed(Keyboard.Key.NoMove, true);
			}

			no_rotate_parent.multiplier *= modifier;
			no_rotate_parent.mass_multiplier *= modifier;
		}

		//		[ISystem.LateUpdate(ISystem.Mode.Single)]
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

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateMovement(ISystem.Info info, ref Region.Data region, [Source.Owned, Override] in Runner.Data runner, [Source.Owned] ref Runner.State runner_state, [Source.Owned] ref Body.Data body, [Source.Owned] in Control.Data control)
		{
			ref readonly var keyboard = ref control.keyboard;

			var time = info.WorldTime;
			var force = new Vector2(0, 0);
			var max_speed = new Vector2(runner.max_speed, runner.max_jump_speed);
			var velocity = body.GetVelocity();
			var mass = body.GetMass();

			var can_move = !keyboard.GetKey(Keyboard.Key.NoMove | Keyboard.Key.X) && !runner_state.flags.HasAny(Runner.Flags.Sitting);
			var is_walking = can_move && keyboard.GetKey(Keyboard.Key.MoveLeft | Keyboard.Key.MoveRight);
			var any = can_move && keyboard.GetKey(Keyboard.Key.MoveLeft | Keyboard.Key.MoveRight | Keyboard.Key.MoveUp | Keyboard.Key.MoveDown);

			if (any || keyboard.GetKeyDown(Keyboard.Key.NoMove | Keyboard.Key.X))
			{
				body.Activate();
			}

			runner_state.flags.SetFlag(Runner.Flags.Walking, is_walking);

			if (is_walking)
			{
				runner_state.last_move = time;
				runner_state.walk_modifier_current = Maths.Lerp(runner_state.walk_modifier_current, 1.00f, runner.walk_lerp);

				if (velocity.X > -runner.max_speed && keyboard.GetKey(Keyboard.Key.MoveLeft)) force.X -= runner.walk_force * runner_state.walk_modifier_current;
				if (velocity.X < +runner.max_speed && keyboard.GetKey(Keyboard.Key.MoveRight)) force.X += runner.walk_force * runner_state.walk_modifier_current;
			}
			else
			{
				runner_state.walk_modifier_current = 0.00f;
			}

			var normal = default(Vector2);
			var layers = default(Physics.Layer);
			var friction = 0.00f;
			var rv = default(Vector2);

			var arbiter_count = 0;
			foreach (var arbiter in body.GetArbiters())
			{
				if (arbiter.GetRigidityDynamic() > 0.90f)
				{
					normal += arbiter.GetNormal();
					friction += arbiter.GetFriction();
					layers |= arbiter.GetLayer();
					rv += arbiter.GetBodyVelocity();
					//arbiter.GetVelocity

					arbiter_count++;
				}
			}

			if (arbiter_count > 0)
			{
				normal /= arbiter_count;
				friction /= arbiter_count;
			}

			normal = normal.GetNormalized();
			runner_state.last_normal = Vector2.Lerp(runner_state.last_normal, normal, 0.20f).GetNormalized();

			var is_grounded = false;
			var is_on_vehicle = false;

			if (arbiter_count > 0) // && !layers.HasAll(Physics.Layer.Bounds))
			{
				//is_grounded = MathF.Abs(normal.X) < 0.35f;


				var dot = MathF.Abs(runner_state.last_normal.X);
				is_grounded = dot < 0.90f;
				is_on_vehicle = dot < 0.75f && layers.HasAny(Physics.Layer.Vehicle | Physics.Layer.Platform);

				//runner_state.uphill_force_current = -MathF.Abs(dot * force.Length()) * friction * 0.50f;
				runner_state.uphill_force_current = -MathF.Abs(dot * force.X);

				force.Y = runner_state.uphill_force_current;
				force.X *= (1.00f - dot);
			}
			else
			{
				force.X *= 0.75f;
				force.Y *= 0.00f;

				force.Y -= runner_state.uphill_force_current * 0.75f;
				runner_state.uphill_force_current *= 0.50f;
			}

//#if CLIENT
//			region.DrawDebugText(body.GetPosition(), $"{rv}\n{runner_state.flags}\n{is_on_vehicle}", Color32BGRA.Yellow);
//#endif

			//#if CLIENT
			//			region.DrawNormal(body.GetPosition(), runner_state.last_normal, is_grounded ? Color32BGRA.Green : Color32BGRA.Yellow);
			//#endif

			if (runner_state.flags.HasAny(Runner.Flags.WallClimbing))
			{
				force.X *= 0.70f;
				force.Y *= 0.20f;
				//runner_state.uphill_force_current *= 0.00f;
			}

			if (is_grounded)
			{
				runner_state.flags.SetFlag(Runner.Flags.Grounded, true);
				runner_state.last_ground = time;
			}
			else
			{
				runner_state.flags.SetFlag(Runner.Flags.Grounded, false);
				runner_state.last_air = time;
			}

			//if (can_move && keyboard.GetKey(Keyboard.Key.MoveUp) && (time - runner_state.last_jump) > 0.40f && (time - runner_state.last_ground) < 0.20f && !runner_state.flags.HasAny(Runner.Flags.WallClimbing))
			if (can_move && keyboard.GetKey(Keyboard.Key.MoveUp) && (time - runner_state.last_jump) > 0.40f && MathF.Min(time - runner_state.last_ground, time - runner_state.last_climb) < 0.20f && !runner_state.flags.HasAny(Runner.Flags.WallClimbing | Runner.Flags.Climbing))
			{
				runner_state.jump_force_current = runner.jump_force;
				runner_state.last_jump = time;
			}

			force.Y -= runner_state.jump_force_current;
			runner_state.jump_force_current *= runner.jump_decay;

			runner_state.flags.SetFlag(Runner.Flags.Crouching, can_move && control.keyboard.GetKey(Keyboard.Key.MoveDown) && (time - runner_state.last_ground) <= 0.25f);
			if (runner_state.flags.HasAll(Runner.Flags.Crouching))
			{
				runner_state.jump_force_current *= 0.50f;
				max_speed.X *= runner.crouch_speed_modifier;
			}

			if (!is_walking && can_move && (velocity - rv).LengthSquared() > 0.10f) // && !is_on_vehicle && (time - runner_state.last_move) <= 0.50f)
			{
				var required_force_dir = ((velocity - rv) * mass * App.tickrate) - force;
				required_force_dir = required_force_dir.GetNormalized(out var required_force_magnitude);
				required_force_dir *= Maths.Clamp(runner.walk_force * 0.50f, -required_force_magnitude, required_force_magnitude);
				required_force_dir.Y = 0.00f;

				if (!runner_state.flags.HasAll(Runner.Flags.Grounded))
				{
					required_force_dir.X *= runner.air_brake_modifier;
				}

				force -= required_force_dir * MathF.Max(1.00f - (time - runner_state.last_move), 0.00f);
			}

			//max_speed *= runner_state.speed_modifier;
			//force *= runner_state.force_modifier;

			runner_state.air_time = time - MathF.Max(runner_state.last_climb, MathF.Max(runner_state.last_ground, runner_state.last_jump));
			runner_state.air_modifier_current = Maths.Lerp(runner_state.air_modifier_current, 1.00f - Maths.Clamp(runner_state.air_time - 0.75f, 0.00f, 1.00f), 0.10f);
			force *= runner_state.air_modifier_current;

			force = Physics.LimitForce(ref body, force, max_speed);

			runner_state.last_force = force;
			body.AddForce(force);

			//#if CLIENT
			//			region.DrawText(body.GetPosition(), $"{runner_state.flags}\n{runner_state.last_jump:0.00}\n{(info.WorldTime - runner_state.last_ground):0.00}", Color32BGRA.White);
			//#endif
		}
	}
}
