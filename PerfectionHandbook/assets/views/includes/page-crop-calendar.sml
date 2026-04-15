<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,0,0" fit="Stretch"/>
  <lane layout="stretch 100%">
    <!-- Crop Calendar -->
    <lane *context={Hovered.CropDetail} padding="30,0,16,8" layout="528px 100%" horizontal-content-alignment="End" orientation="Vertical">
      <lane orientation="Vertical" layout="100% content">
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
            scroll-with-children="Horizontal"
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
      <banner background={@Mods/StardewUI/Sprites/BannerBackground}
        background-border-thickness="24,0"
        margin="0,4,-8,4"
        padding="0,12"
        text={:Seed.DisplayName} />
    </lane>
    <!-- Scroll -->
    <image sprite={@Mods/StardewUI/Sprites/ThinVerticalDivider} layout="content stretch" fit="Stretch"/>
    <!-- Scroll -->
    <scrollable progress={<>ScrollProgress} 
      layout="stretch 100%"
      peeking="128"
      scrollbar-margin="-18,0,0,0"
      z-index="2">
      <grid margin="0,0,4,0" item-layout="length: 72" layout="stretch content">
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
