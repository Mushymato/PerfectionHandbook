<lane layout="stretch stretch" orientation="Vertical">
  <include *context={:this} name="mushymato.PerfectionHandbook/views/includes/goal-infobar" />
  <image sprite={@Mods/StardewUI/Sprites/ThinHorizontalDivider} layout="stretch content" margin="0,4,8,0" fit="Stretch"/>
  <panel layout="stretch 100%">
    <scrollable peeking="128" scroll-step="108"
      progress={<>ScrollProgress}>
      <grid margin="6,0,12,0" item-layout="count: 3" layout="stretch content">
        <frame *repeat={FilteredDisplayPaginated}
          layout="content content"
          padding="24"
          tooltip={:TooltipText}
          background={@mushymato.MachineControlPanel/sprites/cursors:shopBg}
          +hover:background-tint="Wheat">
          <lane orientation="Horizontal">
            <label font="dialogue" layout="stretch content" text={:QuestName}/>
            <label font="dialogue" text={:DisplayCounts}/>
          </lane>
        </frame>
      </grid>
    </scrollable>
  </panel>
</lane>
