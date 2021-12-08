namespace TC2.Base.Components
{
	public static class Human
	{
		[IComponent.Data(Net.SendType.Unreliable)]
		public struct Data: IComponent
		{
			[Save.Ignore, Net.Ignore] public float next_groan;
		}

		public static readonly Sound.Handle[] groan_sounds = new Sound.Handle[]
		{
			"death_long_0",
		};

#if SERVER
		[ISystem.EarlyUpdate(ISystem.Mode.Single)]
		public static void UpdateGroan(ISystem.Info info, [Source.Owned] ref Human.Data human, [Source.Owned] ref Organic.Data organic, [Source.Owned] ref Head.Data head, [Source.Owned] in Transform.Data transform)
		{
			if (info.WorldTime > human.next_groan)
			{
				if (organic.pain_shared > 500.00f && organic.consciousness_shared > 0.30f)
				{
					ref var region = ref info.GetRegion();
					var random = XorRandom.New();
					Sound.Play(ref region, Human.groan_sounds.GetRandom(ref random), transform.position, volume: 0.20f + Maths.Clamp(organic.pain_shared / 2000.00f, 0.00f, 0.50f), pitch: random.NextFloatRange(0.70f, 1.10f) * Maths.Clamp(1.00f + (organic.pain_shared_new / 2000.00f), 0.00f, 0.10f));
					human.next_groan = info.WorldTime + (Maths.Clamp(10.00f - (organic.pain_shared / 300.00f), 5.00f, 10.00f) * random.NextFloatRange(0.80f, 1.30f));
				}
			}
		}
#endif
	}
}
