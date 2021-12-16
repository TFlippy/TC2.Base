
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
		}

		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent
		{
			public float walk_force;
			public float jump_force;
			public float max_speed = 15.00f;

			public float force_modifier = 1.00f;
			public float speed_modifier = 1.00f;

			[Save.Ignore] public float jump_force_current;
			[Save.Ignore] public Runner.Flags flags;

			[Save.Ignore, Net.Ignore] public float last_jump;
			[Save.Ignore, Net.Ignore] public float last_ground;
			[Save.Ignore, Net.Ignore] public float last_air;
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateOrganic(ISystem.Info info, Entity entity,
		[Source.Owned] ref Runner.Data runner, [Source.Owned] in Organic.Data organic)
		{
			runner.force_modifier = organic.strength;
			runner.speed_modifier = organic.efficiency;
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateMovement(ISystem.Info info, Entity entity,
		[Source.Owned] ref Runner.Data runner, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Body.Data body, [Source.Owned] in Control.Data control)
		{
			ref readonly var keyboard = ref control.keyboard;

			var force = new Vector2(0, 0);
			var vel = body.GetVelocity();

			//Walking and Air Movement direction TODO: use two different values for custom usage
			if (!keyboard.GetKey(Keyboard.Key.NoMove))
			{
				if (keyboard.GetKey(Keyboard.Key.MoveLeft) && vel.X > -runner.max_speed) force.X -= runner.walk_force;
				if (keyboard.GetKey(Keyboard.Key.MoveRight) && vel.X < +runner.max_speed) force.X += runner.walk_force;
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

					force.Y = -MathF.Abs(dot * force.X) * friction * 0.50f;
					dot = MathF.Max(0.00f, dot - 0.20f); //Slight bumps are ignored
					force.X *= (1.00f - dot/2.00f);
				}
			}
			
			//Slowed walking while in the air, also no jumping
			if (arbiter_count <= 0 && (info.WorldTime - runner.last_ground) > 0.20f)
			{
				
				force.X *= 0.50f;
				force.Y *= 0.00f;
				runner.flags &= ~Runner.Flags.Grounded;
				runner.last_air = info.WorldTime;
			}

			//JUMP, Coyotee time 0.2s, Jump cooldown 0.4s (Static numbers)
			if (!keyboard.GetKey(Keyboard.Key.NoMove) && keyboard.GetKey(Keyboard.Key.MoveUp) && (info.WorldTime - runner.last_jump) > 0.40f && (info.WorldTime - runner.last_ground) < 0.20f)
			{
				runner.jump_force_current = runner.jump_force;
				runner.last_jump = info.WorldTime;
			}

			//Half jump force each tick
			force.Y -= runner.jump_force_current;
			runner.jump_force_current *= 0.50f;

			//CROUCH
			if (!keyboard.GetKey(Keyboard.Key.NoMove) && control.keyboard.GetKey(Keyboard.Key.MoveDown) && (info.WorldTime - runner.last_ground) <= 0.25f) runner.flags |= Runner.Flags.Crouching;
			else runner.flags &= ~Runner.Flags.Crouching;

			var max_speed = new Vector2(runner.max_speed, 10);

			if (runner.flags.HasAll(Runner.Flags.Crouching)) //TODO: Set max speed to a specific value instead of multiplying
			{
				runner.jump_force_current *= 0.50f;
				max_speed.X *= 0.50f;
			}
		

			//STOP //TODO: use specialized stopping force

			Vector2 velocity = body.GetVelocity();

			if (!runner.flags.HasAll(Runner.Flags.Walking) && (runner.jump_force_current <= 1.00f))
			{
				var required_force_dir = (velocity * body.GetMass() * App.tickrate) - force;
				required_force_dir = required_force_dir.GetNormalized(out var required_force_magnitude);
				required_force_dir *= Maths.Clamp(runner.walk_force * 2.00f, -required_force_magnitude, required_force_magnitude);

				if (!runner.flags.HasAll(Runner.Flags.Grounded)) //No stopping while not on the ground
				{
					required_force_dir.X *= 0.02f;
					required_force_dir.Y *= 0.00f;
				}

				force -= required_force_dir;
			}

			//VELOCITY SPEED REDUCTION (the faster you go the less force your legs add, currently linear)
			if ((velocity.X > 0.00f && force.X > 0.00f) || (velocity.X < 0.00f && force.X < 0.00f))
			{
				force.X *= 1.00f - MathF.Abs(velocity.X / max_speed.X);
			}

			max_speed *= runner.speed_modifier;
			force *= runner.force_modifier;

			//force = Physics.LimitForce(ref body, force, max_speed);
			body.AddForce(force);
		}
	}
}
