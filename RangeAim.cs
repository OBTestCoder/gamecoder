using System;
using System.Collections.Generic;
using ProtoBuf;
using RakNet.Network;
using SapphireEngine;
using SapphireEngine.Functions;
using UnityEngine;
using UServer3.Rust.Data;
using UServer3.Rust.Network;
using UServer3.Rust.Struct;

namespace UServer3.Rust.Functions
{
	// Token: 0x0200001F RID: 31
	public class RangeAim : SapphireType
	{
		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000121 RID: 289 RVA: 0x00009CE8 File Offset: 0x00007EE8
		// (set) Token: 0x06000122 RID: 290 RVA: 0x00009CEF File Offset: 0x00007EEF
		public static RangeAim Instance { get; private set; } = null;

		// Token: 0x06000123 RID: 291 RVA: 0x00009CF8 File Offset: 0x00007EF8
		private static float GetCurrentTime()
		{
			return (float)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
		}

		// Token: 0x06000124 RID: 292 RVA: 0x00009D28 File Offset: 0x00007F28
		public static void NoteFiredProjectile(int projectileID, uint prefabID, int ammotype)
		{
			RangeAim.FiredProjectiles[projectileID] = new FiredProjectile
			{
				FiredTime = RangeAim.GetCurrentTime(),
				PrefabID = prefabID,
				AmmoType = ammotype
			};
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000125 RID: 293 RVA: 0x00009D67 File Offset: 0x00007F67
		private static BasePlayer LocalPlayer
		{
			get
			{
				return BasePlayer.LocalPlayer;
			}
		}

		// Token: 0x06000126 RID: 294 RVA: 0x00009D6E File Offset: 0x00007F6E
		public override void OnAwake()
		{
			RangeAim.Instance = this;
		}

		// Token: 0x06000127 RID: 295 RVA: 0x00009D78 File Offset: 0x00007F78
		public override void OnUpdate()
		{
			this.m_interval_update_tick += SapphireType.DeltaTime;
			bool flag = this.m_interval_update_tick >= 0.1f;
			if (flag)
			{
				this.m_interval_update_tick = 0f;
				bool isHaveLocalPlayer = BasePlayer.IsHaveLocalPlayer;
				if (isHaveLocalPlayer)
				{
					bool flag2 = this.m_no_target_time >= 0.5f;
					if (flag2)
					{
						this.m_no_target_time = 0f;
						this.TargetPlayer = null;
					}
					for (int i = 0; i < BasePlayer.ListPlayers.Count; i++)
					{
						bool flag3 = !BasePlayer.ListPlayers[i].IsLocalPlayer && BasePlayer.ListPlayers[i].IsAlive;
						if (flag3)
						{
							float distance = Vector3.Distance(BasePlayer.ListPlayers[i].Position, BasePlayer.LocalPlayer.Position);
							bool flag4 = (BasePlayer.LocalPlayer.HasActiveItem && OpCodes.IsFireWeapon_Prefab((EPrefabUID)BasePlayer.LocalPlayer.ActiveItem.PrefabID) && distance < 150f) || distance < 70f;
							if (flag4)
							{
								Vector3 forward = BasePlayer.LocalPlayer.GetForward() * distance + BasePlayer.LocalPlayer.EyePos;
								bool flag5 = distance < 10f;
								float distance_check;
								if (flag5)
								{
									distance_check = distance / 2f;
								}
								else
								{
									bool flag6 = distance > 30f;
									if (flag6)
									{
									}
								}
								distance_check = 100f;
								float distance_point_and_playuer = Vector3.Distance(forward, BasePlayer.ListPlayers[i].Position + new Vector3(0f, BasePlayer.ListPlayers[i].GetHeight() * 0.5f, 0f));
								bool flag7 = distance_point_and_playuer < distance_check;
								if (flag7)
								{
									this.m_list_players.Push(new TargetAimInformation
									{
										Player = BasePlayer.ListPlayers[i],
										DistanceCursor = distance_point_and_playuer
									});
								}
							}
						}
					}
					bool flag8 = this.m_list_players.Count > 0;
					if (flag8)
					{
						BasePlayer target = null;
						float dist = float.MaxValue;
						while (this.m_list_players.Count > 0)
						{
							TargetAimInformation player = this.m_list_players.Pop();
							bool flag9 = dist > player.DistanceCursor;
							if (flag9)
							{
								dist = player.DistanceCursor;
								target = player.Player;
							}
						}
						this.TargetPlayer = target;
					}
					else
					{
						bool flag10 = this.TargetPlayer != null;
						if (flag10)
						{
							this.m_no_target_time += 0.1f;
						}
					}
					bool flag11 = this.TargetPlayer != null;
					if (flag11)
					{
						DDraw.Text(this.TargetPlayer.Position + new Vector3(0f, this.TargetPlayer.GetHeight(), 0f), "<size=32>.</size>", Color.red, 0.1f);
					}
				}
			}
		}

		// Token: 0x06000128 RID: 296 RVA: 0x0000A06C File Offset: 0x0000826C
		private static float GetTimeout(FiredProjectile projectile, float distance)
		{
			double maxVelocity = (double)OpCodes.GetMaxVelocity(projectile.AmmoType);
			bool flag = projectile.AmmoType > 0;
			if (flag)
			{
				maxVelocity *= (double)OpCodes.GetProjectileVelocityScale((EPrefabUID)projectile.PrefabID);
			}
			double y = (double)(projectile.FiredTime + 1f);
			double z = maxVelocity;
			double w = (double)OpCodes.GetProjectileInitialDistance(projectile.AmmoType);
			double f = (double)distance;
			double chisl = -w + f + 1.5 * y * z - 0.0979899987578392 * z;
			double znam = 1.5 * z;
			double drob = chisl / znam;
			double normDrob = drob - (double)RangeAim.GetCurrentTime();
			return (float)normDrob;
		}

		// Token: 0x06000129 RID: 297 RVA: 0x0000A110 File Offset: 0x00008310
		public static bool Silent(PlayerProjectileAttack attack)
		{
			bool aimbot_Range_Manual_AutoHeadshot = Settings.Aimbot_Range_Manual_AutoHeadshot;
			if (aimbot_Range_Manual_AutoHeadshot)
			{
			}
			bool flag = RangeAim.Instance.TargetPlayer != null;
			bool result;
			if (flag)
			{
				EHumanBone typeHit = OpCodes.GetTargetHit((EHumanBone)0u, Settings.Aimbot_Range_Manual_AutoHeadshot);
				Vector3 hitPosition = RangeAim.Instance.TargetPlayer.Position;
				float distance = Vector3.Distance(RangeAim.LocalPlayer.EyePos, hitPosition);
				float distance2 = Vector3.Distance(RangeAim.LocalPlayer.Position, attack.playerAttack.attack.hitPositionWorld);
				float timeout = 0f;
				bool flag2 = distance2 < distance;
				if (flag2)
				{
					timeout = RangeAim.GetTimeout(RangeAim.FiredProjectiles[attack.playerAttack.projectileID], distance - distance2);
				}
				bool flag3 = timeout <= 0f;
				if (flag3)
				{
					timeout = 0.001f;
				}
				BasePlayer player = RangeAim.Instance.TargetPlayer;
				PlayerProjectileAttack attackCopy = attack.Copy();
				Timer.SetTimeout(delegate
				{
					RangeAim.SendRangeAttack(player, typeHit, attackCopy, hitPosition);
				}, timeout);
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0600012A RID: 298 RVA: 0x0000A22C File Offset: 0x0000842C
		public static bool Manual(PlayerProjectileAttack attack)
		{
			bool flag = RangeAim.Instance.TargetPlayer != null;
			bool result;
			if (flag)
			{
				EHumanBone typeHit = OpCodes.GetTargetHit((EHumanBone)attack.playerAttack.attack.hitBone, Settings.Aimbot_Range_Manual_AutoHeadshot);
				RangeAim.SendRangeAttack(RangeAim.Instance.TargetPlayer, typeHit, attack, RangeAim.Instance.TargetPlayer.Position);
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0600012B RID: 299 RVA: 0x0000A294 File Offset: 0x00008494
		public static bool SendRangeAttack(BasePlayer target, EHumanBone typeHit, PlayerProjectileAttack parentAttack, Vector3 pos)
		{
			bool isAlive = target.IsAlive;
			if (isAlive)
			{
				parentAttack.hitDistance = Vector3.Distance(target.Position, BasePlayer.LocalPlayer.Position);
				HitInfo hitInfo = OpCodes.GetTargetHitInfo(typeHit);
				parentAttack.playerAttack.attack.hitBone = hitInfo.HitBone;
				parentAttack.playerAttack.attack.hitPartID = hitInfo.HitPartID;
				parentAttack.playerAttack.attack.hitNormalLocal = hitInfo.HitNormalPos;
				parentAttack.playerAttack.attack.hitPositionLocal = hitInfo.HitLocalPos;
				parentAttack.playerAttack.attack.hitID = target.UID;
				float height = target.GetHeight();
				parentAttack.playerAttack.attack.hitPositionWorld = pos;
				parentAttack.playerAttack.attack.hitNormalWorld = pos;
				parentAttack.playerAttack.attack.pointEnd = pos;
				VirtualServer.BaseClient.write.Start();
				VirtualServer.BaseClient.write.PacketID(Message.Type.RPCMessage);
				VirtualServer.BaseClient.write.UInt32(RangeAim.LocalPlayer.UID);
				VirtualServer.BaseClient.write.UInt32(3322107216u);
				PlayerProjectileAttack.Serialize(VirtualServer.BaseClient.write, parentAttack);
				VirtualServer.BaseClient.write.Send(new SendInfo(VirtualServer.BaseClient.Connection));
			}
			return true;
		}

		// Token: 0x040000CB RID: 203
		private static Dictionary<int, FiredProjectile> FiredProjectiles = new Dictionary<int, FiredProjectile>();

		// Token: 0x040000CC RID: 204
		public BasePlayer TargetPlayer = null;

		// Token: 0x040000CD RID: 205
		private float m_interval_update_tick = 0f;

		// Token: 0x040000CE RID: 206
		private float m_no_target_time = 0f;

		// Token: 0x040000CF RID: 207
		private Stack<TargetAimInformation> m_list_players = new Stack<TargetAimInformation>();
	}
}
