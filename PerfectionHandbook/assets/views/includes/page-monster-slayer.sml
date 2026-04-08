<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,8,0" fit="Stretch"/>
  <panel layout="stretch 100%">
    <scrollable peeking="128" progress={<>ScrollProgress}>
      <grid margin="6,0,12,0" item-layout="count: 3" layout="stretch content">
        <frame *repeat={FilteredDisplayPaginated}
          layout="content content"
          padding="12"
          focusable="true"
          tooltip={:TooltipText}
          background={@mushymato.PerfectionHandbook/sprites/cursors:shopBg}>
          <panel>
            <image sprite={@Mods/StardewUI/Sprites/White} tint="#4CAF50" fit="Stretch" layout={QuestFillLayout}/>
            <lane padding="6" orientation="Horizontal" vertical-content-alignment="Middle">
              <label font="dialogue" layout="stretch content" max-lines="1" text={:QuestName} shadow-alpha="0.5" />
              <label font="dialogue" text={:DisplayCounts} max-lines="1" shadow-alpha="0.5" />
            </lane>
          </panel>
        </frame>
      </grid>
    </scrollable>
  </panel>
</lane>
