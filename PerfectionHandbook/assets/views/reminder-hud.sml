<frame layout="320px content" background={@Mods/StardewUI/Sprites/ShopEntryBorder}>
  <lane orientation="vertical" padding="16">
    <label *!if={HasReminders} margin="4" font="small" text={#ui.no-reminders} shadow-alpha="1" max-lines="1"/>
    <lane *repeat={Reminders} orientation="horizontal" vertical-content-alignment="Middle">
      <image sprite={:Icon} layout="32px 32px" margin="2" />
      <label font="small" text={:Text} shadow-alpha="1" max-lines="1"/>
    </lane>
  </lane>
</frame>
