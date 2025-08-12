using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using ZeldackLib.DebugDraw;

namespace ArenaPlus.Features.NPC
{
	public class JumpFinder
	{
		bool visualize = false;
		public JumpFinder(Room room, ArenaNPCPlayer owner, IntVector2 startPos)
		{
			this.room = room;
			this.owner = owner;
			this.startPos = startPos;
			this.startCell = owner.AI.pathFinder.PathingCellAtWorldCoordinate(room.GetWorldCoordinate(startPos));
			this.NewTest();
		}

		public bool BeneficialMovement
		{
			get
			{
				return this.bestJump != null && this.bestJump.goalCell != null && this.startPos.FloatDist(this.bestJump.goalCell.worldCoordinate.Tile) > 5f && this.owner.AI.pathFinder.GetDestination.Tile.FloatDist(this.bestJump.goalCell.worldCoordinate.Tile) > 3f && !Scavenger.JumpFinder.PathWeightComparison(this.bestJump.goalCell, this.owner.jumpCell) && !Scavenger.JumpFinder.PathWeightComparison(this.bestJump.goalCell, this.startCell);
			}
		}

		// Token: 0x060048E1 RID: 18657 RVA: 0x004F9448 File Offset: 0x004F7648
		public void Update()
		{
            if (this.owner.actOnJump == null)
			{
				this.fade++;
				if (!Scavenger.JumpFinder.PathWeightComparison(this.owner.jumpCell, this.startCell))
				{
					this.fade += 10;
				}
				if (this.fade > 40)
				{
					this.Destroy();
				}
			}
			if (this.owner.safariControlled)
			{
				Vector2 zero = Vector2.zero;
				if (this.owner.inputWithDiagonals != null)
				{
					zero = new Vector2(Mathf.Sign((float)this.owner.inputWithDiagonals.Value.x) * (float)((this.owner.inputWithDiagonals.Value.x == 1) ? 1 : 0), Mathf.Sign((float)this.owner.inputWithDiagonals.Value.y) * (float)((this.owner.inputWithDiagonals.Value.y == 1) ? 1 : 0));
				}
				if (zero != this.lastControlledDir)
				{
					this.lastControlledDir = zero;
					this.NewTest();
				}
			}
			for (int i = Mathf.Max(1, 100 / Mathf.Max(1, this.owner.jumpFinders.Count)); i >= 0; i--)
			{
				this.Iterate();
			}
            if (this.visualize)
            {
                RoomDebugDraw.SetupWithColor(new Color(1f, 1f, 1f, 0.5f));
                RoomDebugDraw.DrawRect(room.MiddleOfTile(startPos), Vector2.one * 10f);
                RoomDebugDraw.DrawArrowDir(this.room.MiddleOfTile(this.startPos), this.bestJump.initVel);

				RoomDebugDraw.color = Color.white;
                RoomDebugDraw.DrawRect(pos, Vector2.one * 5f);

            }
            if (this.owner.actOnJump == this)
			{
				PathFinder.PathingCell pathingCell = this.bestJump.goalCell;
				for (int j = 0; j < 8; j++)
				{
					PathFinder.PathingCell pathingCell2 = this.owner.AI.pathFinder.PathingCellAtWorldCoordinate(this.bestJump.goalCell.worldCoordinate + Custom.eightDirections[j]);
					if (Scavenger.JumpFinder.PathWeightComparison(pathingCell, pathingCell2))
					{
						pathingCell = pathingCell2;
					}
				}
				if (pathingCell != this.bestJump.goalCell)
				{
					this.landingDirection = new Vector2?(Custom.DirVec(this.room.MiddleOfTile(this.bestJump.goalCell.worldCoordinate), this.room.MiddleOfTile(pathingCell.worldCoordinate)));
				}
			}
		}

		// Token: 0x060048E2 RID: 18658 RVA: 0x004F964C File Offset: 0x004F784C
		private void Iterate()
		{
			this.lastPos = this.pos;
			this.pos += this.vel;
			this.vel *= 0.999f;
			this.vel.y = this.vel.y - 0.9f;
			int i;
			for (i = SharedPhysics.RayTracedTilesArray(this.lastPos, this.pos, this._cachedRtList); i >= this._cachedRtList.Length; i = SharedPhysics.RayTracedTilesArray(this.lastPos, this.pos, this._cachedRtList))
			{
				Custom.LogWarning(new string[] { string.Format("Scavenger JumpFinder ray tracing limit exceeded, extending cache to {0} and trying again!", this._cachedRtList.Length + 100) });
				Array.Resize<IntVector2>(ref this._cachedRtList, this._cachedRtList.Length + 100);
			}
			Vector2 vector = Custom.PerpendicularVector(this.lastPos, this.pos);
			for (int j = 0; j < i; j++)
			{
				if (this.room.GetTile(this._cachedRtList[j]).Solid || this._cachedRtList[j].y < 0 || this._cachedRtList[j].y < this.room.defaultWaterLevel || this.room.aimap.getAItile(this._cachedRtList[j]).narrowSpace)
				{
					this.NewTest();
					return;
				}
				if (!this.hasVenturedAwayFromTerrain && this.room.aimap.getTerrainProximity(this._cachedRtList[j]) > 1 && !this.room.GetTile(this._cachedRtList[j]).verticalBeam && !this.room.GetTile(this._cachedRtList[j]).horizontalBeam)
				{
					this.hasVenturedAwayFromTerrain = true;
				}
				if (this.hasVenturedAwayFromTerrain && this.room.aimap.TileAccessibleToCreature(this._cachedRtList[j], this.owner.Template) && (this.room.aimap.getTerrainProximity(this._cachedRtList[j]) == 1 || this.room.GetTile(this._cachedRtList[j]).verticalBeam || this.room.GetTile(this._cachedRtList[j]).horizontalBeam) && this.startPos.FloatDist(this._cachedRtList[j]) > (float)Custom.IntClamp((int)(this.currentJump.initVel.magnitude / 3f), 5, 20) && this.owner.AI.pathFinder.GetDestination.Tile.FloatDist(this._cachedRtList[j]) > 3f)
				{
					PathFinder.PathingCell pathingCell = this.owner.AI.pathFinder.PathingCellAtWorldCoordinate(this.room.GetWorldCoordinate(this._cachedRtList[j]));
					if (Scavenger.JumpFinder.PathWeightComparison(this.bestJump.goalCell, pathingCell))
					{
						this.bestJump = this.currentJump;
						this.bestJump.goalCell = pathingCell;
						Vector2 vector2 = this.room.MiddleOfTile(pathingCell.worldCoordinate);
						Vector2 vector3 = Custom.DirVec(this.lastPos, this.pos);
						this.bestJump.grabWhenLanding = false;
						for (int k = -1; k < 2; k++)
						{
							if (!this.room.GetTile(vector2 + Custom.PerpendicularVector(vector3) * 15f + vector3 * 20f).Solid)
							{
								this.bestJump.grabWhenLanding = true;
								break;
							}
						}
                        if (this.visualize)
                        {
							RoomDebugDraw.SetupWithColor(this.bestJump.grabWhenLanding ? new Color(1f, 1f, 1f) : new Color(0f, 0f, 1f));
                            RoomDebugDraw.DrawRect(this.room.MiddleOfTile(pathingCell.worldCoordinate), Vector2.one * 10f);
                            RoomDebugDraw.DrawArrowDir(this.room.MiddleOfTile(this.startPos), this.bestJump.initVel);
                        }
                    }
				}
				if ((!this.room.GetTile(this.startPos + new IntVector2(0, 1)).Solid && this.room.GetTile(this._cachedRtList[j] + new IntVector2(0, 1)).Solid) || this.room.GetTile(this.room.MiddleOfTile(this._cachedRtList[j]) + vector * Custom.LerpMap((float)this.currentJump.tick, 5f, 20f, 10f, 20f)).Solid || this.room.GetTile(this.room.MiddleOfTile(this._cachedRtList[j]) - vector * Custom.LerpMap((float)this.currentJump.tick, 5f, 20f, 10f, 20f)).Solid)
				{
					this.NewTest();
					return;
				}
			}
			this.currentJump.tick++;
			if (this.currentJump.tick > 700)
			{
				this.NewTest();
			}
		}

		private void NewTest()
		{
			float power = global::UnityEngine.Random.value * Mathf.Pow(this.owner.JumpPower, 0.5f);
			if (this.room.aimap.getTerrainProximity(this.startPos) > 1)
			{
				power *= 0.5f;
			}
			float num2 = Mathf.Lerp(14f, 50f, power);
			if (this.owner.grasps[0] != null)
			{
				num2 *= 0.75f;
			}
			Vector2 vector = Custom.DegToVec((45f * Mathf.Pow(global::UnityEngine.Random.value, 0.75f) + 135f * Mathf.Pow(global::UnityEngine.Random.value, 2f)) * ((global::UnityEngine.Random.value >= 0.5f) ? 1f : (-1f))) * num2;
			if (this.owner.safariControlled && this.owner.inputWithDiagonals != null && (this.owner.inputWithDiagonals.Value.x != 0 || this.owner.inputWithDiagonals.Value.y != 0))
			{
				vector = Custom.DegToVec(Custom.VecToDeg(new Vector2((float)this.owner.inputWithDiagonals.Value.x, (float)this.owner.inputWithDiagonals.Value.y)) + 22.5f * global::UnityEngine.Random.value * ((global::UnityEngine.Random.value >= 0.5f) ? 1f : (-1f))) * num2;
			}
			this.currentJump = new Scavenger.JumpFinder.JumpInstruction(this.room.MiddleOfTile(this.startPos), vector, power);
			this.pos = this.room.MiddleOfTile(this.startPos);
			this.lastPos = this.pos;
			this.vel = this.currentJump.initVel;
			this.hasVenturedAwayFromTerrain = false;
			if (this.bestJump == null)
			{
				this.bestJump = this.currentJump;
			}
		}

		public static bool PathWeightComparison(PathFinder.PathingCell A, PathFinder.PathingCell B)
		{
			if (A == null)
			{
				return B != null;
			}
			if (B == null)
			{
				return false;
			}
			if (B.costToGoal.legality != PathCost.Legality.Allowed)
			{
				return false;
			}
			if (B.generation == A.generation)
			{
				return B.costToGoal.resistance < A.costToGoal.resistance;
			}
			return B.generation > A.generation;
		}

		public void Destroy()
		{
			this.slatedForDeletion = true;
		}

		private ArenaNPCPlayer owner;

		public bool slatedForDeletion;

		public Room room;

		public IntVector2 startPos;

		public int fade;

		private PathFinder.PathingCell startCell;

		public Scavenger.JumpFinder.JumpInstruction bestJump;

		public Scavenger.JumpFinder.JumpInstruction currentJump;

		private Vector2 pos;

		private Vector2 lastPos;

		private Vector2 vel;

		private bool hasVenturedAwayFromTerrain;

		public Vector2? landingDirection;

		public Vector2 lastControlledDir;

		private IntVector2[] _cachedRtList = new IntVector2[100];

		public class JumpInstruction
		{
			public JumpInstruction(Vector2 startPos, Vector2 initVel, float power)
			{
				this.startPos = startPos;
				this.initVel = initVel;
				this.power = power;
			}

			public PathFinder.PathingCell goalCell;

			public int tick;

			public Vector2 startPos;

			public Vector2 initVel;

			public float power;

			public bool grabWhenLanding;
		}
	}
}
