import * as React from "react";
import { LogoSvg } from "./svg.component";

export const LogoComponent = ({ classes }) => (
  <div className={classes.container}>
    <LogoSvg className={classes.svg} />
  </div>
);
