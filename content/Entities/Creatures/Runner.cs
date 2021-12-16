
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
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent
		{
			public float walk_force;
			public float jump_force;
			public float stop_force;
			public float max_speed = 13.00f;

			public float force_modifier = 1.00f;
			public float speed_modifier = 1.00f;

			public float crouch_speed_modifier = 0.50f;
			public float air_speed_modifier = 0.50f;

			public float jump_cooldown = 0.40f;

			[Save.Ignore] public float jump_force_current;
			[Save.Ignore] public Runner.Flags flags;

			[Save.Ignore, Net.Ignore] public float last_jump;
			[Save.Ignore, Net.Ignore] public float last_ground;
			[Save.Ignore, Net.Ignore] public float last_climb;
			[Save.Ignore, Net.Ignore] public float last_air;
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateOrganic(ISystem.Info info, Entity entity,
		[Source.Owned] ref Runner.Data runner, [Source.Owned] in Organic.Data organic, [Source.Owned] in Organic.State organic_state)
		{
			runner.force_modifier = MathF.Pow(organic.strength, 0.50f);
			runner.speed_modifier = organic.efficiency;
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateClimbing(ISystem.Info info, Entity entity,
		[Source.Owned] ref Runner.Data runner, [Source.Parent] in Climber.Data climber)
		{
			runner.flags.SetFlag(Runner.Flags.Climbing, climber.cling_entity.IsValid());
			if (runner.flags.HasAll(Runner.Flags.Climbing)) runner.last_climb = info.WorldTime;
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateMovement(ISystem.Info info, Entity entity,
		[Source.Owned] ref Runner.Data runner, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body, [Source.Owned] in Control.Data control)
		{
			ref readonly var keyboard = ref control.keyboard;

			var force = new Vector2(0, 0);
			Vector2 velocity = body.GetVelocity();

			//WALKING
			if (!keyboard.GetKey(Keyboard.Key.NoMove))
			{
				if (keyboard.GetKey(Keyboard.Key.MoveLeft)) force.X -= runner.walk_force;
				if (keyboard.GetKey(Keyboard.Key.MoveRight)) force.X += runner.walk_force;
			}

			if (!keyboard.GetKey(Keyboard.Key.NoMove) && keyboard.GetKey(Keyboard.Key.MoveLeft | Keyboard.Key.MoveRight)) runner.flags |= Runner.Flags.Walking;
			else runner.flags &= ~Runner.Flags.Walking;

			//Find colliding terrain
			var arbiter_count = 0;
			var normal = default(Vector2);
			var layers = default(Physics.Layer);
			var friction = 0.00f;

			foreach (var arbiter in body.GetArbiters())
			{
				normal += arbiter.GetNormal();
				friction += arbiter.GetFriction();
				layers |= arbiter.GetLayerB();

				arbiter_count++;
			}

			//Slope walking (Converts forward movementspeed into up movementspeed)
			if (arbiter_count > 0)
			{
				normal /= arbiter_count;
				friction /= arbiter_count;

				if (normal.Y >= -0.40f && !layers.HasAll(Physics.Layer.Bounds))
				{
					runner.flags |= Runner.Flags.Grounded;
					runner.last_ground = info.WorldTime;

					var dot = Vector2.Dot(normal, new Vector2(MathF.Sign(normal.X), 0));

					force.Y -= MathF.Abs(dot * force.X) * (1.00f - friction) * 0.50f; //friction IMPROVED climbing
					dot = MathF.Max(0.00f, dot - 0.20f); //Slight bumps are ignored
					force.X *= (1.00f - dot/2.00f);
				}
			}
			
			//Slowed walking while in the air, also no jumping, small amount of extra time so small divets dont break your stride
			if (arbiter_count <= 0 && (info.WorldTime - runner.last_ground) > 0.20f)
			{
				force.X *= runner.air_speed_modifier;
				force.Y *= 0.00f; //Currently irrelevant since nothing adds Y speed beforehand
				runner.flags &= ~Runner.Flags.Grounded;
				runner.last_air = info.WorldTime;
			}
			
			//JUMP, Coyotee time 0.2s
			if (!keyboard.GetKey(Keyboard.Key.NoMove) && keyboard.GetKey(Keyboard.Key.MoveUp) && (info.WorldTime - runner.last_jump) > jump_cooldown && (info.WorldTime - runner.last_ground) < 0.20f || (((info.WorldTime - runner.last_climb) > 0.00f) && (info.WorldTime - runner.last_climb) < 0.20f))
			{
				runner.jump_force_current = (runner.jump_force + velocity.Y * body.GetMass() * App.tickrate * 0.25f);
				//Less jump force if already moving upwards (and slightly more when already moving downwards)
				runner.last_jump = info.WorldTime;
			}
			if (!keyboard.GetKey(Keyboard.Key.MoveUp))
			{
				runner.jump_force_current *= 0.10f;
			}

			//Half jump force each tick
			force.Y -= runner.jump_force_current;
			runner.jump_force_current *= 0.50f;

			//CROUCH
			if (!keyboard.GetKey(Keyboard.Key.NoMove) && control.keyboard.GetKey(Keyboard.Key.MoveDown) && (info.WorldTime - runner.last_ground) <= 0.25f) runner.flags |= Runner.Flags.Crouching;
			else runner.flags &= ~Runner.Flags.Crouching;

			var max_speed = new Vector2(runner.max_speed, 10);

			//Crouching reduces max speed, but doesnt reduce jump height
			if (runner.flags.HasAll(Runner.Flags.Crouching))
			{
				//runner.jump_force_current *= 0.50f;
				max_speed.X *= runner.crouch_speed_modifier;
			}
		
		 	//STOP (by touching the ground and neither jumping nor moving)
			if (runner.flags.HasAll(Runner.Flags.Grounded))
			if (!runner.flags.HasAll(Runner.Flags.Walking) && (runner.jump_force_current <= 1.00f))
			{
				var required_force_dir = (velocity * body.GetMass() * App.tickrate) - force;
				required_force_dir = required_force_dir.GetNormalized(out var required_force_magnitude);
				required_force_dir *= Maths.Clamp(runner.stop_force, -required_force_magnitude, required_force_magnitude);

				force -= required_force_dir;
			}

			//VELOCITY SPEED REDUCTION (the faster you go the less force your legs add, currently linear up to max speed)
			if ((velocity.X > 0.00f && force.X > 0.00f) || (velocity.X < 0.00f && force.X < 0.00f))
			{
				force.X *= 1.00f - MathF.Abs(velocity.X / max_speed.X);
			}

			max_speed *= runner.speed_modifier;
			force.X *= runner.force_modifier;
			force.Y *= MathF.Pow(runner.force_modifier, 0.60f); //Reduced jump scaling with strength

			//force = Physics.LimitForce(ref body, force, max_speed); //No longer relevant due to above speed force scaling
			body.AddForce(force);
		}
	}
}
