<panel layout="stretch 100%">
  <!-- Scroll -->
  <scrollable progress={<>ScrollProgress} 
    layout="stretch 100%"
    peeking="128"
    scrollbar-margin="-18,0,0,0"
    z-index="2">
    <grid margin="2" padding="12,4,8,4" item-layout="length: 72+" item-spacing="4,4" layout="stretch content"
      focusable-tag="main-item-grid"
      primary-item-count={>PrimaryItemCount}
      button-press=|HandleShoulderButtons($Button)|
      >
      <panel *repeat={:FilteredDisplayPaginated}
        layout="64px 64px"
        focusable="true"
        horizontal-content-alignment="End"
        vertical-content-alignment="End"
        left-click=|ToggleReminder()|
        pointer-enter=|^HoveredEnter(this)|
        tooltip={Tooltip}
        hovered-subject={:ReprItem}>
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
    </grid>
  </scrollable>
</panel>
