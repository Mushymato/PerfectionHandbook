<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,8,0" fit="Stretch"/>
  <lane layout="stretch 100%">
    <!-- Crop Calendar -->
    <lane *context={Hovered} padding="30,0,16,8" layout="500px 100%" orientation="Vertical">
      <banner text={:Info.Datum.DisplayName} padding="0,8"/>
      <lane *context={:CropDetail} orientation="Vertical">
        <lane *context={:Settings} padding="2">
          <segments highlight={@Mods/StardewUI/Sprites/White}
            highlight-tint="#39d"
            highlight-transition="150ms EaseOutQuart"
            selected-index={<>SpeedGroIdx}>
            <image *repeat={:SpeedGroKinds} sprite={:Info} tooltip={:Tooltip} layout="48px 48px" margin="4"/>
          </segments>
          <checkbox layout="content 32px" margin="12,12,4,4"
            label-text={:LabelAgriculturist}
            is-checked={<>UseAgriculturist} />
        </lane>
        <grid
          margin="8,8"
          layout="content content"
          item-layout="count: 7"
          item-spacing="0,0"
          button-press=|ChangeMonth($Button)|
          wheel=|ScrollMonth($Direction)|>
          <frame *repeat={HarvestCells}
            layout="content content"
            border={@Mods/StardewUI/Sprites/MenuSlotInset}
            border-tint={:CellBorderTint}
            scroll-with-children="Horizontal"
            focusable="true"
            margin="-4"
            border-thickness="12,4"
            left-click=|^ChangeStartDay(this)|>
            <image sprite={:Sprite}
              fit="Contain"
              vertical-alignment="Middle"
              layout="48px 96px"
              shadow-alpha={:DisplayShadow}
              shadow-offset="-4,4"
              z-index="2"
            />
          </frame>
        </grid>
        <label *if={:ShowMonth} font="dialogue" text={DisplayMonth} shadow-alpha="0.8"/>
      </lane>
    </lane>
    <!-- Scroll -->
    <image sprite={@Mods/StardewUI/Sprites/ThinVerticalDivider} layout="content stretch" margin="4,0" fit="Stretch"/>
    <!-- Scroll -->
    <scrollable progress={<>ScrollProgress} 
      layout="stretch 100%"
      peeking="128"
      scrollbar-margin="-26,0,0,0"
      z-index="2">
      <grid margin="4" item-layout="length: 72" layout="stretch content">
        <frame *repeat={FilteredDisplayPaginated}
          border={@Mods/StardewUI/Sprites/MenuSlotOutset}
          border-thickness="10"
          layout="64px 64px"
          border-tint={BorderTint}
          focusable="true">
          <panel
            horizontal-content-alignment="End"
            vertical-content-alignment="End"
            left-click=|^ToggleHoverable(this)|
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
        </frame>
      </grid>
    </scrollable>
  </lane>
</lane>
