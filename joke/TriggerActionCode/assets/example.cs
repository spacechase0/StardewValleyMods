float strength = float.TryParse(args.Length > 0 ? args[0] : "4", out float f) ? f : 4;
Monitor.Log($"strength: {string.Join(" ", args)} {strength}");
Game1.player.jump(strength);
