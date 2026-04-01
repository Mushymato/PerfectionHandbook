<frame layout="1244px 90%[580..]"
  background={@Mods/StardewUI/Sprites/MenuBackground}
  border={@Mods/StardewUI/Sprites/MenuBorder}
  border-thickness="32, 36, 24, 36">
  <scrollable>
    <grid margin="16,0" item-layout="count: 4" layout="stretch content">
      <button *repeat={:PerfectionGoals}
        hover-background={@Mods/StardewUI/Sprites/ButtonLight}
        horizontal-content-alignment="Start"
        vertical-content-alignment="Start"
        margin="4"
        padding="16,12,0,0"
        layout="stretch 200px">
        <panel layout="100% 100%" >
          <label font="dialogue" text={:Goal.DisplayName} shadow-alpha="0.8" />
          <panel layout="100% 100%" padding="0,0,0,0" horizontal-content-alignment="Start" vertical-content-alignment="End">
            <image sprite={:Goal.DisplayIcon} layout="48px 48px" padding="0,12"/>
          </panel>
          <panel layout="100% 100%" padding="0,0,16,12" horizontal-content-alignment="End" vertical-content-alignment="End">
            <label font="dialogue" text={:MyFulfillment.DisplayText} />
          </panel>
        </panel>
      </button>
    </grid>
  </scrollable>
</frame>

<!-- <template name="form-row">
  <lane layout="content content" vertical-content-alignment="middle" margin="0,8">
    <banner layout="content content"
      margin="0,8"
      text={&title}/>
    <outlet />
  </lane>
</template> -->
