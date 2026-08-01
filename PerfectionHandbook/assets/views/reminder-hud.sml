<frame layout="content content" clip-size="content content" background={@Mods/StardewUI/Sprites/ShopEntryBorder}>
  <lane orientation="vertical" padding="16">
    <label *!if={HasReminders} margin="4" font="small" text={#ui.no-reminders} shadow-alpha="0.8" max-lines="1"/>
    <lane *repeat={Reminders} *if={Showing}
      layout="content content"
      orientation="horizontal"
      vertical-content-alignment="Middle"
      left-click=|ToggleSubEntries()|>
      <image *!if={:IsSub}
        margin="6,0" layout="18px 18px"
        sprite={@mushymato.PerfectionHandbook/sprites/cursors:crossBox}
        left-click=|~RemindersContext.RemoveEntryDisplay(this)|
      />
      <image *if={:IsSub} layout="16px 16px" margin="32,8,0,8" sprite={@Mods/StardewUI/Sprites/CaretRight}/>
      <panel horizontal-content-alignment="Middle" vertical-content-alignment="End">
        <image sprite={:Icon} layout="32px 32px" margin="2" fit="Contain" horizontal-alignment="Middle"/>
        <digits *if={:HasCount} scale="2" number={:Count} />
      </panel>
      <label margin="8,0" font="small" text={DisplayText} shadow-alpha="0.8" max-lines="-1"/>
    </lane>
  </lane>
</frame>
