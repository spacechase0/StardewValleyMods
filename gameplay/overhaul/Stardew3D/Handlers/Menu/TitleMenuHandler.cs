using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoScene.Graphics;
using SharpGLTF.Schema2;
using SpaceShared;
using Stardew3D.Data;
using Stardew3D.GameModes.VR;
using Stardew3D.Handlers;
using Stardew3D.Models;
using Stardew3D.Rendering;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Mods;
using static Stardew3D.Handlers.IRenderHandler;
using static Stardew3D.Models.ModelObject;

namespace Stardew3D.Handlers.Menu;
internal class TitleMenuHandler : GenericMenuHandler<TitleMenu>
{
    public TitleMenuHandler(VRGameMode mode, TitleMenu menu)
        : base(mode, menu )
    {
    }

    private bool ShowingMainMenu
    {
        get
        {
            var subMenu = TitleMenu.subMenu;
            if (subMenu != null && subMenu is not AboutMenu && subMenu is not LanguageSelectionMenu)
                return false;

            return true;
        }
    }

    public override void Update(IUpdateHandler.UpdateContext ctx)
    {
        ctx.ForceUpdateIfNotAlreadyRun(ctx);

        var subMenu = TitleMenu.subMenu;
        if (subMenu != null)
        {
            var subHandlers = Stardew3D.Mod.State.GetUpdateHandlersFor(subMenu);
            foreach (var subHandler in subHandlers)
            {
                subHandler?.Update(ctx);
            }
        }
        else
        {
            base.Update(ctx);
        }
    }

    protected override RenderDataBase CreateInitialRenderData(RenderContext ctx)
    {
        return new RenderData(ctx, this);
    }

    private new class RenderData : GenericMenuHandler<TitleMenu>.RenderData
    {
        public ModelObject skybox;

        private ModelObjectInstance skyboxInstance;

        public RenderData(RenderContext ctx, TitleMenuHandler parent)
            : base(ctx, parent)
        {
            // TODO: (?)Use model associations instead
            skybox = Stardew3D.Mod.State.ModelManager.RequestModel("kittycatcasey.Stardew3D/Skybox");

            skyboxInstance = skybox.Draw(Batch, Matrix.Identity);

            Vector3 spot = new(0, 3, -15f);
        }

        public override void Update(RenderContext ctx)
        {
            base.Update(ctx);

            var subMenu = TitleMenu.subMenu;
            if (subMenu != null)
            {
                var subHandlers = Stardew3D.Mod.State.GetRenderHandlersFor(subMenu);
                foreach (var subHandler in subHandlers)
                {
                    subHandler?.Render(ctx);
                }
            }

            //RenderHelper.DebugRenderGrid();

            skybox.Update(Batch, skyboxInstance, Matrix.CreateTranslation(ctx.WorldCamera.Position));
        }
    }
}
