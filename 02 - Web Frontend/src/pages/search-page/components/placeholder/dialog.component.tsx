import * as React from "react";
import IconButton from "material-ui/IconButton";
import MuiDialog, { DialogTitle, DialogContent, DialogContentText, DialogProps } from "material-ui/Dialog";
import CloseIcon from "material-ui-icons/Close";
import Typography from "material-ui/Typography";
import { withTheme } from "material-ui/styles";
import { cnc } from "../../../../util";
import { LinkComponent } from './link.component';
const styles = require('./dialog.styles.scss');
const jfkFilesScenario = require('../../../../assets/img/cogsearch-architecture.png');

const Dialog: React.StatelessComponent<DialogProps> = ({ ...props }) => {
  return (
    <MuiDialog {...props} className={cnc(props.className, styles.dialog)} classes={{ paper: styles.content }}>
      <DialogTitle>
        <div className={styles.titleContainer}>
          <Typography variant="title" classes={{ title: styles.title }}>Explore your files using Cognitive Search with Azure Search</Typography>
          <IconButton onClick={props.onClose}>
            <CloseIcon />
          </IconButton>
        </div>
      </DialogTitle>
      <DialogContent>
        <DialogContentText>
          <span className={styles.block}>
            Using this starter project, you can explore how to use both built-in and custom Cognitive Skills inside of Azure Search to explore your documents, stored in an Azure Storage account. 
            The Cognitive Search capabilities of Azure Search ingests your data from almost multiple datasources and enriches it using a set of cognitive skills that extracts knowledge and then lets you explore the data using Search.
          </span>
          <span className={styles.block}>
            <span>To learn more, read </span>
            <LinkComponent to="https://docs.microsoft.com/en-us/azure/search/cognitive-search-concept-intro">What is "cognitive search" in Azure Search?</LinkComponent>
          </span>
          <span className={styles.block}>
            <span>You can find the source code : </span>
            <LinkComponent to="//aka.ms/jfk-files-code">here</LinkComponent>
          </span>
          <span className={styles.block}>Below is a basic architectural diagram that represents how cognitive search in Azure Search works:</span>
          <img src={jfkFilesScenario} alt="Cognitive search with Azure Search" className={styles.img} />
        </DialogContentText>
      </DialogContent>
    </MuiDialog>
  );
}

export const DialogComponent = Dialog;
