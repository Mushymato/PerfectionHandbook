<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,8,0" fit="Stretch"/>
  <lane layout="stretch 100%">
    <!-- Crop Calendar -->
    <lane *context={Hovered} padding="30,0,16,8" layout="500px 100%" orientation="Vertical">
      <banner text={:Info.Datum.DisplayName} padding="0,8"/>
      <lane *context={:CropDetail}>
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
      <grid margin="4" item-layout="length: 72" item-spacing="4,4" layout="stretch content">
        <frame *repeat={FilteredDisplayPaginated}
          border={@Mods/StardewUI/Sprites/ControlBorder}
          border-thickness="4"
          layout="64px 64px"
          border-tint={BorderTint}
          padding="6"
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
