import * as React from "react";
import { LogoImg } from "./img.component";

export const LogoComponent = ({ classes }) => (
  <div className={classes.container}>
    <LogoImg className={classes.img} />
  </div>
);
