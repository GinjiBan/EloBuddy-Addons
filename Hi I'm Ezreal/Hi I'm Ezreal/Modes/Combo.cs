﻿using System;
using System.Collections.Generic;
using System.Linq;
using AddonTemplate;
using AddonTemplate.Modes;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

using Settings = AddonTemplate.Config.Modes.Combo;

namespace AddonTemplate.Modes
{


    public sealed class Combo : ModeBase
    {
        public override bool ShouldBeExecuted()
        {
            return Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo);
        }

        private void ItemUsage()
        {
            var target = TargetSelector.GetTarget(550, DamageType.Physical); // 550 = Botrk.Range
            if (Settings.UseMuramana && Config.Muramana.IsOwned() && Config.Muramana.IsReady() && !Player.HasBuff("Muramana"))
            {
                Config.Muramana.Cast();
            }
            if (Settings.UseYoumuu && Config.Youmuu.IsOwned() && Config.Youmuu.IsReady())
            {
                Config.Youmuu.Cast();
            }
            if (target != null)
            {
                if (Settings.useBotrk && Item.HasItem(Config.Cutlass.Id) && Item.CanUseItem(Config.Cutlass.Id) &&
                    Player.Instance.HealthPercent < Settings.MinHPBotrk &&
                    target.HealthPercent < Settings.EnemyMinHPBotrk)
                {
                    Item.UseItem(Config.Cutlass.Id, target);
                }
                if (Settings.useBotrk && Item.HasItem(Config.Botrk.Id) && Item.CanUseItem(Config.Botrk.Id) &&
                    Player.Instance.HealthPercent < Settings.MinHPBotrk &&
                    target.HealthPercent < Settings.EnemyMinHPBotrk)
                {
                    Config.Botrk.Cast(target);
                }
            }
        }

        public override void Execute()
        {
            Config.LastComboPressed = Game.Time;
            ItemUsage();
           if (Settings.UseQ && Q.IsReady())
            {
                var target = TargetSelector.GetTarget(Q.Range - 50, DamageType.Physical);
                if (target != null)
                {
                    var predQ = Q.GetPrediction(target);
                    if (predQ.HitChance >= SpellManager.PredQ())
                    {
                        Q.Cast(predQ.CastPosition);
                    }
                }
            }
            if (Settings.UseW && W.IsReady())
            {
                var target = TargetSelector.GetTarget(W.Range - 50, DamageType.Physical);
                if (target != null)
                {
                    var predW = W.GetPrediction(target);
                    if (target != null && predW.HitChance >= SpellManager.PredW())
                    {
                        W.Cast(predW.CastPosition);
                    }
                }
            }
            if (Settings.UseE && E.IsReady())
            {
                var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
                if (target != null)
                {
                    E.Cast(Game.CursorPos);
                }
            }
          if (R.IsReady())
            {
                var heroes = EntityManager.Heroes.Enemies;
                foreach (var hero in heroes.Where(hero => !hero.IsDead && hero.IsVisible && hero.IsInRange(Player.Instance, Config.Modes.Combo.RRange)))
                {
                        if (Settings.UseR && Player.Instance.Position.CountAlliesInRange(2000) <= 3 &&
                            hero.IsKillable(SpellSlot.R) &&
                            !hero.IsKillable(SpellSlot.Q) &&
                            !hero.IsKillable(SpellSlot.W) &&
                            hero.Distance(Player.Instance) > Settings.MinRRange && hero.Distance(Player.Instance) < Settings.RRange)
                        {
                            R.Cast(hero);
                        }
                    
                        if (Settings.UseRSeveral)
                        {
                            var collision = new List<AIHeroClient>();
                            var startPos = Player.Instance.Position.To2D();
                            var endPos = hero.Position.To2D();
                            collision.Clear();
                            foreach (
                                var colliHero in
                                    heroes.Where(
                                        colliHero =>
                                            !colliHero.IsDead && colliHero.IsVisible &&
                                            colliHero.IsInRange(hero, Config.Modes.Combo.RRange)))
                            {
                                    if (Prediction.Position.Collision.LinearMissileCollision(colliHero, startPos, endPos,
                                        SpellManager.R.Speed, SpellManager.R.Width, SpellManager.R.CastDelay))
                                    {
                                        collision.Add(colliHero);
                                    }
                                    if (collision.Count >= Settings.NumberR)
                                    {
                                        R.Cast(hero);
                                    }
                        }
                    }
                }
           }
        }
    }
}
