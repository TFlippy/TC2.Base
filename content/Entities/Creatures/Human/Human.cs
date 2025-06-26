namespace TC2.Base.Components
{
	public static partial class Human
	{
		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public partial struct Data: IComponent
		{
			[Save.Ignore, Net.Ignore] public float next_pain = default;

			public Data()
			{

			}
		}

		public static Sound.Handle[] snd_cough =
		{
			"kobold.cough.00",
			"kobold.cough.01",
			"kobold.cough.02",
			"kobold.cough.03",
			"kobold.grunt.00",
		};

		public static Sound.Handle[] snd_oof =
		{
			"human.oof.00",
			"human.oof.01",
			"human.oof.02",
			"human.oof.03",
			"human.oof.04",
		};

		public static Sound.Handle[] snd_pain_fast =
		{
			"human.pain.fast.00",
			"human.pain.fast.01",
			"human.pain.fast.02"
		};

		public static Sound.Handle[] snd_pain_slow =
		{
			"human.pain.slow.00",
			"human.pain.slow.01",
			"human.pain.slow.02",
			"human.pain.slow.03",
		};

		[ISystem.EarlyUpdate(ISystem.Mode.Single, ISystem.Scope.Region), HasTag("human", true, Source.Modifier.Owned)]
		public static void UpdateHuman(ISystem.Info info, [Source.Owned] ref Human.Data human)
		{

		}

#if SERVER
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region)]
		[HasTag("dead", false, Source.Modifier.Owned), HasTag("human", true, Source.Modifier.Parent)]
		public static void HeadUpdate(ISystem.Info info, Entity entity, ref XorRandom random, ref Region.Data region,
		[Source.Parent, Override] in Organic.Data organic, [Source.Parent] ref Organic.State organic_state, [Source.Parent] ref Human.Data human,
		[Source.Owned] ref Transform.Data transform, [Source.Owned] ref Head.Data head, [Source.Owned] ref Head.State head_state, [Source.Owned] ref Body.Data body)
		{
			var time = info.WorldTime;

			//App.WriteLine("hm");

			if (organic_state.unconscious_time > 0.50f)
			{
				head_state.t_next_sound = Maths.Min(head_state.t_next_sound, time + 2.50f);
			}

			var pain_delta = Maths.Max(organic_state.pain, 0.00f);
			if (time >= human.next_pain && organic_state.consciousness_shared > 0.40f)
			{
				//if (pain_delta >= 800.00f)
				//{
				//	if (random.NextBool(0.90f))
				//	{
				//		Sound.Play(ref region, Kobold.snd_scream.GetRandom(ref random), transform.position, volume: 0.45f, pitch: random.NextFloatRange(0.60f, 0.80f) * head.voice_pitch);
				//		head_state.next_sound = time + random.NextFloatRange(1.50f, 3.00f);
				//	}
				//	human.next_pain = time + 3.00f;
				//}
				//else if (pain_delta >= 300.00f)
				//{
				//	if (random.NextBool(0.90f))
				//	{
				//		Sound.Play(ref region, snd_oof.GetRandom(ref random), transform.position, volume: 0.45f, pitch: random.NextFloatRange(0.90f, 1.10f) * head.voice_pitch);
				//		head_state.next_sound = time + random.NextFloatRange(1.50f, 2.00f);
				//	}
				//	human.next_pain = time + 1.00f;
				//}
				//else if (organic_state.pain >= 200.00f)
				//{
				//	if (random.NextBool(0.20f))
				//	{
				//		Sound.Play(ref region, snd_pain_slow.GetRandom(ref random), transform.position, volume: 0.45f, pitch: random.NextFloatRange(0.90f, 1.10f) * head.voice_pitch);
				//		head_state.next_sound = time + random.NextFloatRange(3.50f, 6.00f);
				//	}
				//	human.next_pain = time + 3.00f;
				//}
			}

			if (time >= head_state.t_next_sound)
			{
				if (organic_state.consciousness_shared > 0.10f && (organic_state.unconscious_time > 3.00f || (organic_state.efficiency < 0.50f && organic_state.pain > 50.00f)))
				{
					var lerp = Maths.Normalize01Fast(organic_state.unconscious_time, 10.00f);

					Sound.Play(ref region, snd_cough.GetRandom(ref random), transform.position, volume: 0.35f * Maths.Lerp(1.00f, 0.50f, lerp), pitch: random.NextFloatRange(0.80f, 1.00f) * Maths.Lerp(1.00f, 0.80f, lerp) * head.voice_pitch);
					head_state.t_next_sound = time + random.NextFloatRange(1.50f, 2.50f + lerp);

					body.AddImpulse(random.NextUnitVector2Range(50.00f, 150.00f));
				}
				else
				{
					if (organic_state.efficiency < 0.60f)
					{
						if (organic_state.pain > 2000.00f)
						{

						}
					}
					else
					{

					}
				}
			}
		}
#endif
	}
}
