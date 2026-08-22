<lane layout="stretch 100%">
  <!-- Crop Calendar -->
  <lane *context={Hovered.CropDetail} padding="30,0,16,8" layout="528px 100%" horizontal-content-alignment="End" orientation="Vertical">
    <lane orientation="Vertical" layout="100% content">
      <lane focusable-tag="side-panel-title" vertical-content-alignment="Middle" focusable="true">
        <image sprite={:Seed}
          shadow-alpha="0.35"
          layout="48px 48px"
          margin="8"
          shadow-offset="-4,4"
          +transition:scale="100ms EaseInSine"
          horizontal-alignment="Middle"
        />
        <label font="dialogue" text={:Seed.DisplayName} shadow-alpha="0.8"/>
      </lane>
      <lane *context={:Settings} margin="4" vertical-content-alignment="Middle">
        <frame border={@Mods/StardewUI/Sprites/MenuSlotTransparent} border-thickness="4">
          <segments highlight={@Mods/StardewUI/Sprites/White}
            highlight-tint="#11bd2e"
            highlight-transition="150ms EaseOutQuart"
            selected-index={<>SpeedGroIdx}>
            <image *repeat={:SpeedGroKinds} sprite={:Info} tooltip={:Tooltip} layout="48px 48px" margin="4"/>
          </segments>
        </frame>
        <frame 
          border={@Mods/StardewUI/Sprites/MenuSlotTransparent} border-thickness="4"
          background={@Mods/StardewUI/Sprites/White}
          background-tint="Transparent"
          +state:checked={UseAgriculturist}
          +state:checked:background-tint="#11a0bd"
        >
          <checkbox layout="48px 48px" margin="4"
            tooltip={:LabelAgriculturist}
            checked-sprite={@mushymato.PerfectionHandbook/sprites/cursors:agriculturist}
            unchecked-sprite={@mushymato.PerfectionHandbook/sprites/cursors:agriculturist}
            is-checked={<>UseAgriculturist}
            opacity="0.5"
            +state:checked={UseAgriculturist}
            +state:checked:opacity="1"/>
        </frame>
        <frame border={@Mods/StardewUI/Sprites/MenuSlotTransparent} border-thickness="4">
          <segments highlight={@Mods/StardewUI/Sprites/White}
            highlight-tint="#bd114a"
            highlight-transition="150ms EaseOutQuart"
            selected-index={<>^Month}>
            <image *repeat={:^CropSeasonSprites}
              sprite={:Sprite}
              tooltip={:Name}
              vertical-alignment="Middle"
              fit="Contain"
              layout="48px 48px"
              margin="4"/>
          </segments>
        </frame>
      </lane>
      <grid
        margin="12,8,0,0"
        layout="content content"
        item-layout="count: 7"
        item-spacing="0,0"
        button-press=|ChangeMonth($Button)|
        wheel=|ScrollMonth($Direction)|>
        <frame *repeat={HarvestCells}
          layout="content content"
          border={@Mods/StardewUI/Sprites/MenuSlotInset}
          border-tint={:CellBorderTint}
          focusable="true"
          margin="-4,-2"
          border-thickness="14,4"
          left-click=|^ChangeStartDay(this)|>
          <panel>
            <image *if={:ShowDirt}
                sprite={@mushymato.PerfectionHandbook/sprites/hoeDirt:base}
                fit="Contain"
                vertical-alignment="End"
                layout="48px 96px"
              />
            <image *if={:ShowDirt}
              sprite={@mushymato.PerfectionHandbook/sprites/hoeDirt:wet}
              fit="Contain"
              vertical-alignment="End"
              layout="48px 96px"
            />
            <image *if={:ShowPaddy}
              sprite={@mushymato.PerfectionHandbook/sprites/hoeDirt:paddy}
              fit="Contain"
              vertical-alignment="End"
              layout="48px 96px"
            />
            <image *if={:IsHarvest}
              sprite={:^Basket}
              fit="Contain"
              vertical-alignment="End"
              layout="48px 80px"
              margin="0,8"
            />
            <image *if={:IsHarvest}
              sprite={:Sprite}
              fit="Contain"
              vertical-alignment="End"
              layout="48px 70px"
              z-index="2"
            />
            <image *!if={:IsHarvest}
              sprite={:Sprite}
              fit="Contain"
              vertical-alignment="Middle"
              layout="48px 96px"
              z-index="2"
            />
          </panel>
        </frame>
      </grid>
    </lane>
  </lane>
  <!-- Divider -->
  <image sprite={@Mods/StardewUI/Sprites/ThinVerticalDivider} layout="content stretch" fit="Stretch"/>
  <!-- Scroll -->
  <scrollable progress={<>ScrollProgress} 
    layout="stretch 100%"
    peeking="128"
    scrollbar-margin="-18,0,0,0"
    z-index="2">
    <grid margin="0,0,4,0" item-layout="length: 72" layout="stretch content"
        primary-item-count={>PrimaryItemCount}
        button-press=|HandleShoulderButtons($Button)|>
      <frame *repeat={FilteredDisplayPaginated}
        focusable-tag={:ViewName}
        border={@Mods/StardewUI/Sprites/MenuSlotOutset}
        border-thickness="10"
        layout="64px 64px"
        border-tint={BorderTint}
        focusable="true"
        left-click=|^ToggleHoverable(this)|
        pointer-enter=|^HoveredEnter(this)|
        tooltip={Tooltip}
        hovered-subject={:ReprItem}>
        <panel
          horizontal-content-alignment="End"
          vertical-content-alignment="End">
          <image sprite={:Info.Datum}
            tint={DisplayTint}
            shadow-alpha={DisplayShadow}
            scale={DisplayScale}
            layout="64px 64px"
            shadow-offset="-4,4"
            +transition:scale="100ms EaseInSine"
          />
          <digits *if={HasCount} scale="3" number={Count} />
          <image *if={Reminder.Active} sprite={@mushymato.PerfectionHandbook/sprites/cursors:blueExclaim} layout="12px 32px" margin="0,0,52,32" />
        </panel>
      </frame>
    </grid>
  </scrollable>
</lane>
