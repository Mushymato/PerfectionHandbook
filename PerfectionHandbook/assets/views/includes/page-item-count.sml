<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,8,0" fit="Stretch"/>
  <lane layout="stretch 100%">
    <!-- Scroll -->
    <scrollable progress={<>ScrollProgress} 
      layout="stretch 100%"
      peeking="128"
      scrollbar-margin="-26,0,0,0"
      z-index="2">
      <grid margin="2" padding="12,4,8,4" item-layout="length: 72+" item-spacing="4,4" layout="stretch content">
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
  </lane>
</lane>
