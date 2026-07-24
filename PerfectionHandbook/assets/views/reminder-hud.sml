<frame layout="content content" background={@Mods/StardewUI/Sprites/ShopEntryBorder}>
  <lane orientation="vertical" padding="16">
    <label *!if={HasReminders} margin="4" font="small" text={#ui.no-reminders} shadow-alpha="0.8" max-lines="1"/>
    <lane *repeat={Reminders} layout="content content" orientation="horizontal" vertical-content-alignment="Middle">
      <image *if={:IsSub} layout="16px 16px" margin="8,8,0,8" sprite={@Mods/StardewUI/Sprites/CaretRight}/>
      <panel horizontal-content-alignment="Middle" vertical-content-alignment="End">
        <image sprite={:Icon} layout="32px 32px" margin="2" fit="Contain" horizontal-alignment="Middle"/>
        <digits *if={:HasCount} scale="2" number={:Count} />
      </panel>
      <label margin="8,0" font="small" text={:Text} shadow-alpha="0.8" max-lines="-1"/>
    </lane>
  </lane>
</frame>
