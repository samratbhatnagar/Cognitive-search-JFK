import * as React from "react";
import { Item } from "../../view-model";
import { Chevron } from "../../../../common/components/chevron";
import { HocrPreviewComponent } from "../../../../common/components/hocr";
import Card, { CardActions, CardContent, CardMedia } from "material-ui/Card";
import List, { ListItem, ListItemIcon, ListItemText } from "material-ui/List";
import Collapse from "material-ui/transitions/Collapse";
import Typography from "material-ui/Typography";
import Chip from "material-ui/Chip";
import StarIcon from "material-ui-icons/Star";
import { cnc } from "../../../../util";

const style = require("./item.style.scss");
const columnStyle = require("../../../../common/components/json-reader/json-reader.style.scss");

interface ItemProps {
  item: Item;
  listMode?: boolean;
  activeSearch?: string;
  targetWords?: string[];
  onClick?: (item: Item) => void;
  simplePreview?: boolean;
}
interface ListProps {
  title: string;
  fields: string[];
}
interface State {
  expanded: boolean;
}

const handleOnClick = ({ item, onClick }) => () =>
  onClick ? onClick(item) : null;

const ratingStars = (item: Item) =>
  item.rating >= 1.0
    ? Array(Math.floor(item.rating))
        .fill(0)
        .map((item, index) => (
          <StarIcon
            key={index}
            classes={{ root: style.star }}
            color="secondary"
          />
        ))
    : null;

const ItemMediaThumbnail: React.StatelessComponent<ItemProps> = ({
  item,
  onClick
}) => {
  return item.thumbnail ? (
    <CardMedia
      className={style.media}
      image={item.thumbnail}
      title={item.title}
      onClick={handleOnClick({ item, onClick })}
    />
  ) : null;
};
const ItemMediaOffice: React.StatelessComponent<ItemProps> = ({
  item,
  onClick
}) => {
  return (
    <div>
      <iframe
        src={getOfficePreviewPath(item.filePath)}
        className={cnc(style.media, style.officePreview)}
      />
    </div>
  );
};
const ItemMediaHocrPreview: React.StatelessComponent<ItemProps> = ({
  item,
  activeSearch,
  targetWords,
  onClick
}) => {
  const isPhoto = item.type && item.type.toLowerCase() === "photo";
  return (
    <div
      className={isPhoto ? style.mediaExtended : style.media}
      onClick={handleOnClick({ item, onClick })}
    >
      <HocrPreviewComponent
        hocr={item.metadata}
        pageIndex={item.demoInitialPage}
        zoomMode={isPhoto ? "original" : "page-width"}
        targetWords={targetWords}
        renderOnlyTargetWords={true}
        disabelScroll={true}
      />
    </div>
  );
};

const ItemMediaJsonPreview: React.StatelessComponent<ItemProps> = ({
  item,
  activeSearch,
  targetWords,
  onClick
}) => {
  let parsedItem: any = null;

  if (typeof item.metadata === "string") {
    try {
      parsedItem = JSON.parse(item.metadata);
    } catch {}
  }

  const myObj = parsedItem || item.metadata;
  const itemData = Object.keys(myObj);
  if (typeof myObj !== "object") {
    // Returns as text block
    return (
      <div
        className={cnc(style.media, style.jsonTextBlock)}
        onClick={handleOnClick({ item, onClick })}
      >
        <div
          dangerouslySetInnerHTML={{
            __html: item.highlightPreview
              ? item.highlightPreview.reduce(
                  (acc, cur) => acc + cur + "...&nbsp;"
                )
              : ""
          }}
        />
      </div>
    );
  } else {
    return (
      // Returns as new UI
      <div
        className={cnc(style.media, columnStyle.jsonGridSummary)}
        onClick={handleOnClick({ item, onClick })}
      >
        <div className={columnStyle.column}>
          <ul>
            <li>
              {itemData.map((child, index) => {
                return (
                  <div
                    key={index}
                    className={cnc(
                      columnStyle.lineItemDetail,
                      typeof myObj[child] === "object"
                        ? columnStyle.hasChildren
                        : null
                    )}
                  >
                    <label>{child}</label>
                    <span>
                      {typeof myObj[child] !== "object" ? myObj[child] : ""}
                    </span>
                  </div>
                );
              })}
            </li>
          </ul>
        </div>
      </div>
    );
  }
};

const ItemMedia: React.StatelessComponent<ItemProps> = ({
  item,
  activeSearch,
  targetWords,
  onClick,
  simplePreview
}) => {
  if (true) {
    return (
      <ItemMediaJsonPreview
        item={item}
        activeSearch={activeSearch}
        targetWords={targetWords}
        onClick={onClick}
      />
    );
  } else {
    return simplePreview ? (
      <ItemMediaThumbnail item={item} onClick={onClick} />
    ) : isOfficeType(item.filePath) ? (
      <ItemMediaOffice item={item} onClick={onClick} />
    ) : (
      <ItemMediaHocrPreview
        item={item}
        activeSearch={activeSearch}
        targetWords={targetWords}
        onClick={onClick}
      />
    );
  }
};

const ItemCaption: React.StatelessComponent<ItemProps> = ({
  item,
  onClick
}) => {
  let { title } = item;
  if (title.length > 150) {
    title = title.slice(0, 150);
    title += "...";
  }
  return (
    <CardContent
      classes={{ root: style.caption }}
      onClick={handleOnClick({ item, onClick })}
    >
      <Typography variant="headline" component="h2" color="inherit">
        {title}
        <span className={style.subtitle}>{item.subtitle}</span>
      </Typography>
      <Typography component="p" color="inherit">
        {item.excerpt}
      </Typography>
    </CardContent>
  );
};

const isOfficeType = (path: string) => {
  if (!path) return false;
  const pathLower = path.toLocaleLowerCase();
  return (
    pathLower.includes(".doc") ||
    pathLower.includes(".ppt") ||
    pathLower.includes(".xls")
  );
};
const getOfficePreviewPath = (path: string) => {
  return (
    "https://view.officeapps.live.com/op/view.aspx?src=" +
    encodeURIComponent(path)
  );
};
const getConsolidatedFields = field => {
  const consolidatedFields = field.reduce((acc, cur) => {
    if (acc[cur]) acc[cur] += 1;
    else acc[cur] = 1;
    return acc;
  }, {});
  return consolidatedFields;
};
const getOrderedTags = consolidatedFields => {
  const orderedKeys = Object.keys(consolidatedFields).sort((a, b) => {
    return consolidatedFields[b] - consolidatedFields[a];
  });
  return orderedKeys;
};
const generateExtraFieldContent = (consolidatedFields, orderedKeys) => {
  return (
    <div className={style.tagContainer}>
      {orderedKeys.map((tag, tagIndex) => (
        <Chip
          label={`${tag}${
            consolidatedFields[tag] > 1
              ? "(" + consolidatedFields[tag] + ")"
              : ""
          }`}
          key={tagIndex}
          classes={{ root: style.tag }}
        />
      ))}
    </div>
  );
};

const generateExtraField = (field: any, index: number) => {
  const consolidatedFields = getConsolidatedFields(field);
  const orderedKeys = getOrderedTags(consolidatedFields);
  return field ? (
    <ListItem key={index}>
      {generateExtraFieldContent(consolidatedFields, orderedKeys)}
    </ListItem>
  ) : null;
};
const ItemExtraFieldList: React.StatelessComponent<ListProps> = ({
  title,
  fields
}) => {
  return (
    <CardContent>
      <h4 className={style.listTitle}>{title}</h4>
      <List>{generateExtraField(fields, 0)}</List>
    </CardContent>
  );
};

// const ItemHeadingsList: React.StatelessComponent<ItemProps> = ({ item }) => {
//   if (item.headings) {
//     return (
//       <CardContent>
//         <h4 className={style.listTitle}>Headings</h4>
//         <List>
//           {item.headings.map((field, fieldIndex) =>
//             generateExtraField(field, fieldIndex)
//           )}
//         </List>
//       </CardContent>
//     );
//   } else {
//     return null;
//   }
// };

export class ItemComponent extends React.Component<ItemProps, State> {
  constructor(props) {
    super(props);

    this.state = {
      expanded: false
    };
  }

  private toggleExpand = () => {
    this.setState({
      ...this.state,
      expanded: !this.state.expanded
    });
  };

  public render() {
    const { item, activeSearch, targetWords, onClick } = this.props;
    const combined = item.extraFields.reduce((acc, cur) => {
      if (cur instanceof Array) return acc.concat(cur);
      acc.push(cur);
      return acc;
    }, []);
    const consolidatedFields = getConsolidatedFields(combined);
    const orderedKeys = getOrderedTags(consolidatedFields);
    const topFields = [];
    if (orderedKeys.length > 3) {
      topFields[0] = consolidatedFields[orderedKeys[0]];
      topFields[1] = consolidatedFields[orderedKeys[1]];
      topFields[2] = consolidatedFields[orderedKeys[2]];
    }
    return (
      <Card
        classes={{
          root: cnc(style.card, this.props.listMode && style.listMode)
        }}
        elevation={8}
      >
        <ItemCaption item={item} onClick={onClick} />
        <ItemMedia
          item={item}
          activeSearch={activeSearch}
          targetWords={targetWords}
          onClick={onClick}
        />
        {generateExtraFieldContent(topFields, orderedKeys.slice(0, 3))}

        <CardActions classes={{ root: style.actions }}>
          <div className={style.rating}>{ratingStars(item)}</div>
          <Chevron
            className={style.chevron}
            onClick={this.toggleExpand}
            expanded={this.state.expanded}
          />
        </CardActions>
        <Collapse
          classes={{ container: style.collapse }}
          in={this.state.expanded}
          timeout="auto"
          unmountOnExit
        >
          <ItemExtraFieldList title={"Entities"} fields={combined} />
          <ItemExtraFieldList title={"Headings"} fields={item.headings} />
        </Collapse>
      </Card>
    );
  }
}
