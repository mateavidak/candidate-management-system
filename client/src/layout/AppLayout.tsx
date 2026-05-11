import { NavLink, Outlet } from 'react-router-dom'
import './AppLayout.css'

export function AppLayout() {
  return (
    <div className="shell">
      <aside className="shell__aside">
        <div className="shell__brand">
          <span className="shell__logo" aria-hidden>
            HR
          </span>
          <div>
            <div className="shell__title">Candidate Hub</div>
            <div className="shell__subtitle">HR platform</div>
          </div>
        </div>
        <nav className="shell__nav" aria-label="Main">
          <NavLink to="/candidates" className="shell__link" end>
            Candidates
          </NavLink>
          <NavLink to="/skills" className="shell__link">
            Skills
          </NavLink>
        </nav>
      </aside>
      <div className="shell__main">
        <Outlet />
      </div>
    </div>
  )
}
