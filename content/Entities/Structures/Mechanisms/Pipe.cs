
namespace TC2.Base.Components
{
	public static class Air
	{
		public static partial class Container
		{
			[IComponent.Data(Net.SendType.Unreliable, region_only: true), ITrait.Data(Net.SendType.Unreliable, region_only: true)]
			public struct Data: IComponent, ITrait
			{
				[Flags]
				public enum Flags: uint
				{
					None = 0u,
				}

				public Amount moles_o2;
				public Amount moles_h2;
				public Amount moles_n2;
				public Amount moles_co2;

				public Amount moles_co;
				public Amount moles_so2;
				public Amount moles_no2;
				public Amount moles_h2o;

				public Temperature temperature = Temperature.Ambient;
				public Volume volume = 1.00f.m3();

				public Air.Container.Data.Flags flags;

				public Data()
				{

				}
			}
		}
	}

	public static partial class Vent
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true), ITrait.Data(Net.SendType.Reliable, region_only: true)]
		public struct Data: IComponent, ITrait
		{
			[Flags]
			public enum Flags: uint
			{
				None = 0u,

				Has_Pipe = 1u << 0
			}

			public Inventory.Type type;
			public Vent.Data.Flags flags;

			[Editor.Picker.Position(true, true)] 
			public Vector2 offset;
			[Editor.Picker.Direction(true, true)]
			public Vector2 direction = new(0, -1);

			public Area cross_section = Area.Circle(10.00f.cm());

			//public float velocity;
			public float flow_rate;

			[Save.Ignore, Net.Ignore] public float t_next_smoke;

			public Data()
			{

			}
		}

		public static float GetFlowVelocity(ref readonly this Vent.Data vent)
		{
			Phys.VolumetricFlowRate(vent.cross_section, out var velocity, vent.flow_rate);
			return velocity;
		}
	}

	public static partial class Pipe
	{
		[IComponent.Data(Net.SendType.Reliable, region_only: true)]
		public struct Data: IComponent, ILink
		{
			public readonly Entity EntityA => this.a.entity;
			public readonly Entity EntityB => this.b.entity;
			public readonly ulong ComponentA => this.a.ComponentID;
			public readonly ulong ComponentB => this.b.ComponentID;

			public float scale_multiplier = 1.00f;
			public float length;
			public Area cross_section = Area.Circle(10.00f.cm());

			public EntRef<Vent.Data> a;
			public EntRef<Vent.Data> b;

			public Data()
			{

			}
		}

		//[IRelation.Data(Net.SendType.Reliable, IRelation.Flags.Acyclic | IRelation.Flags.Reflexive | IRelation.Flags.Tag)]
		//public struct Link: IRelation
		//{

		//}

		[IComponent.Data(Net.SendType.Unreliable, region_only: true)]
		public struct State: IComponent
		{
			public Pressure pressure;
			public Volume volume;
			public Volume flow;

			[Net.Ignore, Save.Ignore] public float next_tick;
		}

#if SERVER
		[ISystem.LateUpdate(ISystem.Mode.Single, ISystem.Scope.Region, interval: 0.20f)]
		public static void OnLateUpdate(ISystem.Info info, Entity entity, ref XorRandom random, [Source.Owned] in Pipe.Data pipe, [Source.Owned] ref Pipe.State pipe_state, [Source.Owned] in Transform.Data transform)
		{
			var ts = Timestamp.Now();

			if (info.WorldTime >= pipe_state.next_tick)
			{
				var sync = false;

				if (sync)
				{
					//entity.SyncComponent(ref pipe_state);
					pipe_state.Sync(entity);
				}
			}
		}
#endif

#if CLIENT
		[ISystem.PreRender(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateResizable(ISystem.Info info, Entity entity, [Source.Owned] ref Pipe.Data pipe, [Source.Owned] ref Pipe.State pipe_state, [Source.Owned] ref Resizable.Data resizable, [Source.Owned] ref Transform.Data transform)
		{
			if (resizable.flags.HasAny(Resizable.Flags.Dynamic) && pipe.a.TryGetHandle(out var vent_a) && pipe.b.TryGetHandle(out var vent_b))
			{
				ref var transform_a = ref pipe.a.entity.GetComponent<Transform.Data>();
				ref var transform_b = ref pipe.b.entity.GetComponent<Transform.Data>();

				if (transform_a.IsNotNull() && transform_b.IsNotNull())
				{
					var pos_a = transform_a.LocalToWorld(vent_a.data.offset);
					var pos_b = transform_b.LocalToWorld(vent_b.data.offset);

					resizable.a = transform.WorldToLocal(pos_a);
					resizable.b = transform.WorldToLocal(pos_b);

					//App.WriteLine("test");

					//resizable.cap_a_rotation = transform_a.LocalToWorldRotation(MathF.PI);
					//resizable.cap_b_rotation = transform_b.LocalToWorldRotation(0.00f);
					//entity.GetRegion().DrawDebugText(transform.position, $"{vent_a.data.direction}; {vent_a.data.direction.GetAngleRadians()}", Color32BGRA.White);


					resizable.cap_a_rotation = transform_a.LocalToWorldDirection(vent_a.data.direction).GetAngleRadiansFast(); // transform_a.LocalToWorldDirection(vent_a.data.direction).GetAngleRadiansFast(); //.LocalToWorldRotation(MathF.PI);
					resizable.cap_b_rotation = transform_b.LocalToWorldDirection(vent_b.data.direction).GetAngleRadiansFast();
				}
			}
		}

		[ISystem.PreRender(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void UpdateRenderer(ISystem.Info info, Entity entity, [Source.Owned] ref Pipe.Data pipe, [Source.Owned] ref Pipe.State pipe_state, [Source.Owned] ref Rope.Renderer.Data renderer, [Source.Owned] ref Transform.Data transform)
		{
			if (pipe.a.TryGetHandle(out var vent_a) && pipe.b.TryGetHandle(out var vent_b))
			{
				ref var transform_a = ref pipe.a.entity.GetComponent<Transform.Data>();
				ref var transform_b = ref pipe.b.entity.GetComponent<Transform.Data>();

				if (transform_a.IsNotNull() && transform_b.IsNotNull())
				{
					var pos_a = transform_a.LocalToWorld(vent_a.data.offset);
					var pos_b = transform_b.LocalToWorld(vent_b.data.offset);

					var delta = (pos_b - pos_a);
					var dir = delta.GetNormalized(out var len);

					//var rect = new AABB(pos_a, pos_b);

					//App.WriteLine("test");

					renderer.p0 = pos_a + transform_a.LocalToWorldDirection(vent_a.data.direction * 0.250f);
					renderer.p3 = pos_b + transform_b.LocalToWorldDirection(vent_b.data.direction * 0.250f);

					//renderer.p1 = pos_a + (new Vector2(pos_b.X - pos_a.X, 0.00f));
					//renderer.p2 = renderer.p1 + new Vector2(0.00f, 0.00f);

					renderer.p1 = pos_a + transform_a.LocalToWorldDirection(vent_a.data.direction * 2.50f);
					renderer.p2 = pos_b + transform_b.LocalToWorldDirection(vent_b.data.direction * 2.50f);

					//renderer.p1 = new Vector2(pos_a.X, pos_b.Y);
					//renderer.p2 = new Vector2(pos_b.X, pos_a.Y);

					//var a = Vector2.Lerp(renderer.p0, renderer.p3, 0.10f);
					//var b = Vector2.Lerp(renderer.p3, renderer.p0, 0.10f);

					//var dir = renderer.p0 - renderer.p3;
					//dir = dir.GetNormalized(out var len);

					//var (sin, cos) = MathF.SinCos(App.GetCurrentTime());

					//var len = (Vector2.Distance(renderer.p0, renderer.p1) + Vector2.Distance(renderer.p2, renderer.p3)) * 0.50f;

					//App.WriteLine("test");

					//if (info.WorldTime >= pipe_state.next_tick)
					//{
					//	pipe_state.next_tick = info.WorldTime + (pipe.interval * 0.10f);
					//	pipe_state.load -= pipe_state.load * 1.60f; // Maths.Lerp(pipe_state.load, 0.00f, 0.30f);
					//}

					//a += dir * load;
					//b += dir * load;

					//renderer.p1 = Vector2.Lerp(renderer.p1, a, 0.40f);
					//renderer.p2 = Vector2.Lerp(renderer.p2, b, 0.40f);

					//var sag = Maths.Max(0.00f, (len - 2.00f) * 0.10f);

					//renderer.p1 = Vector2.Lerp(renderer.p1, renderer.p2, 0.20f);
					//renderer.p2 = Vector2.Lerp(renderer.p2, renderer.p1, 0.20f);

					//renderer.p1 += new Vector2(0, sag * 4);
					//renderer.p2 += new Vector2(0, sag * 2); // + (dir * load);

					//renderer.z = (entity.id & ECS.id_mask_entity) * 0.0005f;

					renderer.scale = len * pipe.scale_multiplier;
				}
			}
		}
#endif


#if SERVER
		[ISystem.Modified<Pipe.Data>(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnModified(ISystem.Info info, Entity entity, [Source.Owned] ref Pipe.Data pipe, [Source.Owned] ref Pipe.State pipe_state, [Source.Owned] ref Transform.Data transform, [Source.Owned] ref Resizable.Data resizable)
		{
			if (pipe.a.TryGetHandle(out var vent_a) && pipe.b.TryGetHandle(out var vent_b))
			{
				vent_a.data.flags.AddFlag(Vent.Data.Flags.Has_Pipe);
				vent_b.data.flags.AddFlag(Vent.Data.Flags.Has_Pipe);

				ref var transform_a = ref pipe.a.entity.GetComponent<Transform.Data>();
				ref var transform_b = ref pipe.b.entity.GetComponent<Transform.Data>();

				if (transform_a.IsNotNull() && transform_b.IsNotNull())
				{
					var pos_a = transform_a.LocalToWorld(vent_a.data.offset);
					var pos_b = transform_b.LocalToWorld(vent_b.data.offset);

					transform.position = pos_a; // (pos_a + pos_b) * 0.50f;
					transform.Sync(entity);

					resizable.a = transform.WorldToLocal(pos_a);
					resizable.b = transform.WorldToLocal(pos_b);
					resizable.Modified(entity, true);

					//var shape_ptr = entity.GetTraitUnsafe<Body.Data, Shape.Line>();
					//if (shape_ptr != null)
					//{
					//	shape_ptr->a = transform.WorldToLocal(pos_a);
					//	shape_ptr->b = transform.WorldToLocal(pos_b);

					//	entity.MarkModified<Body.Data, Shape.Line>();

					//	entity.SyncTrait<Body.Data, Shape.Line>(shape_ptr);
					//}
				}

				vent_a.Sync();
				vent_b.Sync();
			}
		}

		[ISystem.Remove(ISystem.Mode.Single, ISystem.Scope.Region)]
		public static void OnRemove(ISystem.Info info, Entity entity, [Source.Owned] ref Pipe.Data pipe, [Source.Owned] ref Pipe.State pipe_state)
		{
			if (pipe.a.TryGetHandle(out var vent_a))
			{
				vent_a.data.flags.RemoveFlag(Vent.Data.Flags.Has_Pipe);
				vent_a.Sync();
			}

			if (pipe.b.TryGetHandle(out var vent_b))
			{
				vent_b.data.flags.RemoveFlag(Vent.Data.Flags.Has_Pipe);
				vent_b.Sync();
			}
		}
#endif
	}

}
