<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,0,0" fit="Stretch"/>
  <lane layout="stretch 100%">
    <!-- Scroll -->
    <scrollable progress={<>ScrollProgress} 
      layout="stretch 100%"
      peeking="128"
      scrollbar-margin="-18,0,0,0"
      z-index="2">
      <grid margin="12,12,16,12" item-layout="length: 96+" item-spacing="12,8" layout="stretch content">
        <frame *repeat={FilteredDisplayPaginated} border={@Mods/StardewUI/Sprites/MenuSlotInset} border-thickness="4">
          <lane 
            orientation="Vertical"
            layout="content content"
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
              layout="96px 64px"
              shadow-offset="-4,4"
              +transition:scale="100ms EaseInSine"
              horizontal-alignment="Middle"
              margin="0,8,0,4"
            />
            <digits scale="3" margin="0,2,6,4" tint={DigitTint} number={NeededCount} />
            <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" fit="Stretch"/>
            <digits scale="3" margin="0,2,6,4" tint={DigitTint} number={Count} />
          </lane>
        </frame>
      </grid>
    </scrollable>
  </lane>
</lane>
