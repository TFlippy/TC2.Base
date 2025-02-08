﻿
namespace TC2.Base.Components
{
	public static partial class Dive
	{
		[Flags]
		public enum Flags: ushort
		{
			None = 0,

			//Use_For_Movement = 1 << 0
		}

		[IComponent.Data(Net.SendType.Reliable, IComponent.Scope.Region), IComponent.With<Dive.State>()]
		public partial struct Data(): IComponent
		{
			[Editor.Picker.Position(true, false)]
			public Vector2 offset;
			[Save.Force] public float cooldown = 1.00f;
			[Save.Force] public float speed = 20.00f;

			[Save.NewLine]
			[Save.Force] public Sound.Handle sound;
			[Save.Force] public Dive.Flags flags;
		}

		[IComponent.Data(Net.SendType.Unreliable, IComponent.Scope.Region)]
		public partial struct State(): IComponent
		{
			[Net.Ignore, Save.Ignore] public float next_dive;
		}

		//[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		//public static void UpdateAI(ISystem.Info info, ref XorRandom random, ref Region.Data region,
		//[Source.Owned, Original] ref Animal.Data animal, [Source.Owned] ref Animal.State animal_state,
		//[Source.Owned] in Transform.Data transform, [Source.Owned] ref Control.Data control,
		//[Source.Owned, Original] ref NPC.Data npc_original, [Source.Owned, Override] ref NPC.Data npc_override)
		//{

		//}

		[ISystem.VeryLateUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("dead", false, Source.Modifier.Owned)]
		public static void Update(ISystem.Info info, ref Region.Data region, ref XorRandom random,
		[Source.Owned] in Dive.Data dive, [Source.Owned] ref Dive.State dive_state,
		[Source.Owned] ref Body.Data body, [Source.Owned] in Control.Data control, [Source.Owned] in Transform.Data transform,
		[Source.Owned, Override] in Organic.Data organic, [Source.Owned] in Organic.State organic_state)
		{
			//#if CLIENT
			//			region.DrawDebugDir(pos, dir, Color32BGRA.Red);
			//#endif
			var time = info.WorldTime;
			if (time >= dive_state.next_dive && control.mouse.GetKey(Mouse.Key.Right) && organic_state.consciousness_shared > 0.60f)
			{
				dive_state.next_dive = time + dive.cooldown;

				//var dir = transform.GetDirection(); // (control.mouse.position - pos).GetNormalized();

				var pos = transform.LocalToWorld(dive.offset);
				var dir = (control.mouse.position - pos).GetNormalized();

				var force = dir * body.GetMass() * dive.speed * App.tickrate * organic_state.efficiency;
				force = Physics.LimitForce(ref body, force, new Vector2(dive.speed, dive.speed));

				body.AddForceWorld(force, pos + (dir * 0.50f));
				body.AddVelocity(new Vector2(0, -5 * MathF.Abs(dir.X)));

#if SERVER
				Sound.Play(ref region, dive.sound, pos, volume: 0.80f, pitch: 1.00f, size: 2.00f, priority: 0.60f);
#endif
			}
		}
	}
}
