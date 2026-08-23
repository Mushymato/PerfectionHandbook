<lane layout="stretch 100%">
  <!-- Scroll -->
  <scrollable progress={<>ScrollProgress} 
    layout="stretch 100%"
    peeking="128"
    scrollbar-margin="-18,0,0,0"
    z-index="2">
    <grid margin="12,12,16,12" item-layout="length: 72+" item-spacing="12,8" layout="stretch content"
        primary-item-count={>PrimaryItemCount}
        button-press=|HandleShoulderButtons($Button)|>
        <lane *repeat={:FilteredDisplayPaginated} 
          orientation="Vertical"
          layout="content content"
          focusable="true"
          horizontal-content-alignment="End"
          vertical-content-alignment="End"
          left-click=|ToggleReminder()|
          pointer-enter=|^HoveredEnter(this)|
          screen-read={ScreenRead}
          tooltip={Tooltip}
          hovered-subject={:ReprItem}>
          <panel layout="72px 64px" margin="0,8,0,4">
            <image sprite={:NeededFor.Repr}
              tint={DisplayTint}
              shadow-alpha={DisplayShadow}
              scale={DisplayScale}
              layout="72px 64px"
              shadow-offset="-4,4"
              +transition:scale="100ms EaseInSine"
              horizontal-alignment="Middle"
            />
            <image *if={Reminder.Active} sprite={@mushymato.PerfectionHandbook/sprites/cursors:blueExclaim} layout="12px 32px" margin="8,0,0,0" />
          </panel>
          <digits scale="3" margin="0,-24,6,4" tint={DigitTint} number={NeededCount} />
          <image sprite={@Mods/StardewUI/Sprites/White} tint={DigitTint} layout="stretch 2px" fit="Stretch" shadow-alpha="1" shadow-offset="0,2"/>
          <digits scale="3" margin="0,4,6,4" tint={DigitTint} number={Count} />
        </lane>
    </grid>
  </scrollable>
</lane>
