<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,8,0" fit="Stretch"/>
  <lane layout="stretch 100%">
    <!-- Scroll -->
    <scrollable progress={<>ScrollProgress} 
      layout="stretch 100%"
      peeking="128"
      scrollbar-margin="-14,0,0,0"
      scrollbar-visibility="Visible"
      z-index="2"
      left-click=|ToggleTooltip()|>
      <grid margin="2" item-layout="length: 72" item-spacing="4,4" layout="stretch content">
        <panel *repeat={FilteredDisplayPaginated}
          layout="64px 64px"
          focusable="true"
          horizontal-content-alignment="End"
          vertical-content-alignment="End"
          pointer-enter=|^HoveredEnter(this)|>
          <image sprite={:Info.Datum}
            tooltip={Tooltip}
            hovered-subject={:ReprItem}
            tint={DisplayTint}
            shadow-alpha={DisplayShadow}
            scale={DisplayScale}
            layout="64px 64px"
            shadow-offset="-4,4"
            +transition:scale="100ms EaseInSine"
          />
          <digits *if={HasCount} scale="3" number={Count} />
        </panel>
      </grid>
    </scrollable>
    <!-- Detail -->
    <lane *if={:ShowDetail} *context={Hovered} padding="30,-12,16,8" layout="400px 100%" orientation="Vertical">
      <banner background={@Mods/StardewUI/Sprites/BannerBackground}
        background-border-thickness="32,0"
        text={:Info.Datum.DisplayName}
        padding="0,8"/>
      <label text={:Info.Datum.Description} max-lines="3"/>
      <!-- crop calendar -->
      <grid *if={:HasCropDetail} *context={:CropDetail}
        margin="0,8"
        layout="392px 256px"
        item-layout="length: 56"
        horizontal-item-alignment="middle">
        <frame *repeat={:HarvestCells}
          border={@Mods/StardewUI/Sprites/MenuSlotTransparent}
          border-thickness="4">
          <panel>
            <image fit="Contain" vertical-alignment="Middle" sprite={:Sprite} layout="48px 96px"/>
          </panel>
        </frame>
      </grid>
    </lane>
  </lane>
</lane>
