<frame layout="content content" background={@Mods/StardewUI/Sprites/ShopEntryBorder}
  opacity="0"
  +show:opacity="1"
  +transition:opacity="300ms EaseOutCubic"
  >
  <lane orientation="vertical" padding="16">
    <label *!if={HasReminders} margin="4" font="small" text={#ui.no-reminders} shadow-alpha="0.8" max-lines="1"/>
    <lane *repeat={:Reminders} *context={:Display} orientation="vertical">
      <!-- main entry -->
      <lane layout="content 36px"
        orientation="horizontal"
        vertical-content-alignment="Middle"
        left-click=|ToggleSubEntries()|>
        <image margin="6,0" layout="18px 18px"
          sprite={@mushymato.PerfectionHandbook/sprites/cursors:crossBox}
          left-click=|~RemindersContext.RemoveEntryDisplay(this)|
        />
        <panel horizontal-content-alignment="Middle" vertical-content-alignment="End">
          <image sprite={:Icon} layout="32px 32px" margin="2" fit="Contain" horizontal-alignment="Middle"/>
          <digits *if={:HasCount} scale="2" number={:Count} />
        </panel>
        <label margin="8,0" font="small" text={DisplayText} shadow-alpha="0.8" max-lines="-1"/>
      </lane>
      <!-- sub entries -->
      <lane *repeat={:CastedSubReminders}
        layout="content 0px"
        opacity="0"
        +state:showing={^SubDisplayed}
        +state:showing:opacity="1"
        +state:showing:layout="content 36px"
        +transition:opacity="300ms EaseOutCubic"
        +transition:layout="300ms EaseOutCubic"
        >
        <image layout="16px 16px" margin="32,8,0,8" sprite={@Mods/StardewUI/Sprites/CaretRight}/>
        <panel horizontal-content-alignment="Middle" vertical-content-alignment="End">
          <image sprite={:Icon} layout="32px 32px" margin="2" fit="Contain" horizontal-alignment="Middle"/>
          <digits *if={:HasCount} scale="2" number={:Count} />
        </panel>
        <label margin="8,0" font="small" text={DisplayText} shadow-alpha="0.8" max-lines="-1"/>
      </lane>
    </lane>
  </lane>
</frame>
