<lane layout="stretch 100%">
  <!-- Fish Where -->
  <scrollable *context={Hovered} scrollbar-visibility="Hidden">
    <lane layout="528px content" orientation="vertical">
      <lane focusable-tag="side-panel-title" vertical-content-alignment="Middle" focusable="true">
        <image sprite={:Info.Datum}
          shadow-alpha="0.35"
          layout="48px 48px"
          margin="8"
          shadow-offset="-4,4"
          +transition:scale="100ms EaseInSine"
          horizontal-alignment="Middle"
        />
        <label font="dialogue" text={:Info.Datum.DisplayName} shadow-alpha="0.8"/>
      </lane>
      <lane *repeat={:CanCatchIn} *switch={:IsCrabPot} orientation="horizontal" vertical-content-alignment="Middle" margin="4" focusable="true" opacity={:Opacity}>
        <frame *case="false" border={@Mods/StardewUI/Sprites/MenuSlotTransparent} border-thickness="4">
          <grid item-layout="count: 2" layout="72px content">
            <image *repeat={:SpawnSeasonSprites} layout="36px 24px" sprite={:Sprite} tint={:DisplayTint} />
          </grid>
        </frame>
        <frame *case="false" *if={:HasSpawnWeather} border={@Mods/StardewUI/Sprites/MenuSlotTransparent} border-thickness="4">
          <grid primary-orientation="vertical" item-layout="count: 2" layout="content content[..48]">
            <image *repeat={:SpawnWeatherSprites} layout="36px 24px" sprite={:this}/>
          </grid>
        </frame>
        <lane *case="false" orientation="vertical" margin="8,0">
          <panel *if={:HasSpawnMinFishingLevel} vertical-content-alignment="End">
            <image layout="30px 30px" sprite={@mushymato.PerfectionHandbook/sprites/cursors:fishLv} />
            <digits margin="8,0,0,0" scale="3" number={:SpawnMinFishingLevel} />
            <label margin="40,0,0,0" text={:LocationName} shadow-alpha="0.8"/>
          </panel>
          <label *!if={:HasSpawnMinFishingLevel} text={:LocationName} shadow-alpha="0.8"/>
          <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="440px 2px" margin="-4,4,0,0" fit="Stretch"/>
          <lane *repeat={:SpawnTimeRangeText}>
            <label text={:Text} shadow-alpha="0.8" />
            <image *if={:WithinTime} margin="4,0" layout="27px 27px" sprite={@mushymato.PerfectionHandbook/sprites/cursors_1_6:checkmark} />
          </lane>
        </lane>
        <frame *case="true" border={@Mods/StardewUI/Sprites/MenuSlotTransparent} border-thickness="4" padding="12,0">
          <image layout="48px 48px" sprite={:CrabPotIcon} />
        </frame>
        <label *case="true" text={:CrabPot} margin="8,0" shadow-alpha="0.8" />
      </lane>
    </lane>
  </scrollable>
  <!-- Divider -->
  <image sprite={@Mods/StardewUI/Sprites/ThinVerticalDivider} layout="content stretch" fit="Stretch"/>
  <!-- Scroll -->
  <scrollable progress={<>ScrollProgress}
    layout="stretch 100%"
    peeking="128"
    scrollbar-margin="-18,0,0,0"
    z-index="2">
    <grid margin="2" padding="12,4,8,4" item-layout="length: 72+" item-spacing="4,4" layout="stretch content"
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
        <panel layout="64px 64px"
          horizontal-content-alignment="End"
          vertical-content-alignment="End">
          <image sprite={:Info.Datum}
            tint={DisplayTint}
            shadow-alpha={DisplayShadow}
            scale={DisplayScale}
            layout="64px 64px"
            shadow-offset="-4,4"
            +transition:scale="100ms EaseInSine"
            horizontal-alignment="Middle"
          />
          <digits *if={HasCount} scale="3" number={Count} />
          <image *if={Reminder.Active} sprite={@mushymato.PerfectionHandbook/sprites/cursors:blueExclaim} layout="12px 32px" margin="0,0,52,32" />
        </panel>
      </frame>
    </grid>
  </scrollable>
</lane>
