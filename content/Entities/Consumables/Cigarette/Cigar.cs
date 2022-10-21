
namespace TC2.Base.Components
{
	public static partial class Smoker
	{
		public static readonly Texture.Handle texture_smoke = "BiggerSmoke_Light";

		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public Sprite sprite;
			public uint count;

			[Net.Ignore, Save.Ignore]
			public float t_next_smoke;
		}


#if CLIENT
		[ISystem.PostRender(ISystem.Mode.Single)]
		public static void OnDraw(ISystem.Info info, Entity entity, [Source.Parent] ref Smoker.Data smoker, [Source.Owned] in Transform.Data transform, [Source.Owned] ref Animated.Renderer.Data renderer, [Source.Owned] ref Head.Data head)
		{
			ref var region = ref info.GetRegion();
			var random = XorRandom.New();

			var sprite = smoker.sprite;
			sprite.frame.X = smoker.count - 1;

			var lerp = Maths.NormalizeClamp(smoker.count - 1, 4.00f);

			Animated.Renderer.Draw(transform, new()
			{
				sprite = sprite,
				z = renderer.z + 1.10f,
				rotation = renderer.rotation + Maths.Lerp(0.20f, -0.20f, lerp),
				scale = new Vector2(0.65f),
				offset = renderer.offset + head.offset_mouth
			});

			if (info.WorldTime >= smoker.t_next_smoke)
			{
				smoker.t_next_smoke = info.WorldTime + random.NextFloatRange(0.35f, 0.50f);

				Particle.Spawn(ref region, new Particle.Data()
				{
					texture = texture_smoke,
					lifetime = random.NextFloatRange(1.50f, 3.00f),
					pos = transform.LocalToWorld(head.offset_mouth + new Vector2(0.20f, -0.10f)),
					vel = (new Vector2(0.50f * transform.scale.GetParity(), -0.50f) * random.NextFloatRange(0.50f, 1.50f)) + random.NextVector2(0.50f),
					fps = 8,
					frame_count = 64,
					frame_count_total = 64,
					frame_offset = random.NextByteRange(0, 64),
					scale = random.NextFloatRange(0.05f, 0.10f) * Maths.Lerp(1.00f, 2.50f, lerp),
					rotation = random.NextFloat(10.00f),
					angular_velocity = random.NextFloat(0.50f),
					growth = 0.20f,
					color_a = new Color32BGRA((byte)(120 * Maths.Lerp(1.00f, 1.70f, lerp)), 240, 240, 240),
					color_b = new Color32BGRA(0, 30, 30, 35),
				});
			}
		}
#endif
	}

	public static partial class Cigar
	{
		[IComponent.Data(Net.SendType.Reliable)]
		public partial struct Data: IComponent
		{
			public Sprite sprite_cigar;
		}


#if SERVER
		[ISystem.Event<Consumable.ConsumeEvent>(ISystem.Mode.Single)]
		public static void OnConsume(ISystem.Info info, Entity entity, ref Consumable.ConsumeEvent data, [Source.Owned] in Consumable.Data consumable, [Source.Owned] ref Cigar.Data cigar)
		{
			ref var smoker_new = ref data.ent_organic.GetOrAddComponent<Smoker.Data>(sync: true);
			smoker_new.sprite = cigar.sprite_cigar;

			smoker_new.count = Maths.Clamp(smoker_new.count + 1, 0, 4);
		}
#endif
	}
}