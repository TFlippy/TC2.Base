
namespace TC2.Base.Components
{
	public static partial class Flyer
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public partial struct Data: IComponent
		{
			public float max_speed = 10.00f;
			public float force = 100.00f;
			public float brake_modifier = 0.75f;

			public float sound_speed_modifier = 0.02f;
			public float sound_volume = 0.10f;

			public float lift_modifier = 1.00f;
			public float force_modifier = 1.00f;
			public float speed_modifier = 1.00f;

			public Data()
			{

			}
		}

		[ISystem.Update(ISystem.Mode.Single)]
		public static void UpdateOrganic(ISystem.Info info, Entity entity,
		[Source.Owned] ref Flyer.Data flyer, [Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state)
		{
			flyer.force_modifier = organic.strength;
			flyer.lift_modifier = organic_state.efficiency > 0.20f ? 1.00f : 0.00f;
			flyer.speed_modifier = Maths.Snap(organic_state.efficiency, 0.10f);
		}

		[ISystem.LateUpdate(ISystem.Mode.Single)]
		public static void UpdateMovement(ISystem.Info info, [Source.Owned] ref Flyer.Data flyer, [Source.Owned] in Control.Data control, [Source.Owned] ref Body.Data body)
		{
			ref var region = ref info.GetRegion();
			ref readonly var keyboard = ref control.keyboard;

			var force = new Vector2(0, 0);
			var vel = body.GetVelocity();

			if (!keyboard.GetKey(Keyboard.Key.NoMove))
			{
				if (keyboard.GetKey(Keyboard.Key.MoveLeft) && vel.X > -flyer.max_speed) force.X -= flyer.force;
				if (keyboard.GetKey(Keyboard.Key.MoveRight) && vel.X < +flyer.max_speed) force.X += flyer.force;
				if (keyboard.GetKey(Keyboard.Key.MoveUp) && vel.Y > -flyer.max_speed) force.Y -= flyer.force;
				if (keyboard.GetKey(Keyboard.Key.MoveDown) && vel.Y < +flyer.max_speed) force.Y += flyer.force;
			}

			var max_speed = new Vector2(flyer.max_speed, flyer.max_speed);

			var dir = force.GetNormalized(out var len);
			if (len == 0.00f)
			{
				var required_force_dir = (body.GetVelocity() * body.GetMass() * App.tickrate) - force;
				required_force_dir = required_force_dir.GetNormalized(out var required_force_magnitude);
				force -= required_force_dir *= Maths.Clamp(flyer.force * flyer.brake_modifier, -required_force_magnitude, required_force_magnitude);
			}

			max_speed *= flyer.speed_modifier;
			force *= flyer.force_modifier;
			force = Physics.LimitForce(ref body, force, max_speed);

			if (flyer.lift_modifier > 0.50f)
			{
				force -= body.GetMass() * region.GetGravity() * flyer.lift_modifier;
			}

			body.AddForce(force);
		}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single)]
		public static void UpdateSound(ISystem.Info info, [Source.Owned] in Flyer.Data flyer, [Source.Owned] in Body.Data body, [Source.Owned] ref Sound.Emitter sound_emitter)
		{
			var vel_len = body.GetVelocity().Length();
			sound_emitter.volume = ((flyer.sound_volume * 0.50f) + Maths.Clamp(vel_len * flyer.sound_speed_modifier, 0.00f, flyer.sound_volume * 0.50f)) * flyer.lift_modifier;
			sound_emitter.pitch = 0.80f + Maths.Clamp(vel_len * flyer.sound_speed_modifier, 0.00f, 0.30f);
		}
	}
}
