import * as React from "react";
import { ItemComponent } from "./item.component";
import { ItemCollection, Item } from "../../view-model";
import { cnc, getUniqueStrings } from "../../../../util";

const style = require("./item-collection-view.style.scss");

const TEST_DATA = [
  {
    title: "test title",
    subtitle: "",
    thumbnail: "",
    excerpt: "",
    rating: 0,
    extraFields: [["test", "data"]],
    metadata:
      "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris tincidunt, nisi ut tempor luctus, ligula elit dictum metus.",
    demoInitialPage: 0,
    type: "json",
    highlightWords: ["Hello", "Sir"],
    headings: [["Lorem", "ipsum", "dolor", "sit", "amet"]],
    text: "Lorem ipsum dolor sit amet"
  },
  {
    title: "test title 2",
    subtitle: "",
    thumbnail: "",
    excerpt: "",
    rating: 0,
    extraFields: [["test2", "data2", "more", "information"]],
    metadata: { Name: "Abc", Age: 15, Location: "Bangalore" },
    demoInitialPage: 0,
    type: "json",
    highlightWords: ["Hello", "Sir"],
    headings: [["Lorem", "ipsum"]],
    text: "Lorem ipsum dolor sit amet"
  },
  {
    title: "test title 3",
    subtitle: "",
    thumbnail: "",
    excerpt: "",
    rating: 0,
    extraFields: [["test3", "data3", "more", "information", "third"]],
    metadata: {
      id: 1,
      name: "node1",
      other: "thing",
      children: [{ name: "child1", id: 2 }, { name: "child2", id: 3 }]
    },
    demoInitialPage: 0,
    type: "json",
    highlightWords: ["Hello", "Sir"],
    headings: [["Lorem", "ipsum", "dolor", "sit"]],
    text: "Lorem ipsum dolor sit amet"
  }
];

interface ItemViewProps {
  items?: ItemCollection;
  listMode?: boolean;
  activeSearch?: string;
  targetWords: string[];
  onClick?: (item: Item) => void;
}

export class ItemCollectionViewComponent extends React.Component<
  ItemViewProps,
  {}
> {
  public constructor(props) {
    super(props);
  }

  private injectHighlightWords = (
    targetWords: string[],
    highlightWords: string[]
  ): string[] => {
    return getUniqueStrings([...targetWords, ...highlightWords]);
  };

  public render() {
    return (
      <div
        className={cnc(
          style.container,
          this.props.listMode && style.containerList
        )}
      >
        {this.props.items
          ? this.props.items.map((child, index) => (
              <ItemComponent
                item={child}
                listMode={this.props.listMode}
                activeSearch={this.props.activeSearch}
                targetWords={this.injectHighlightWords(
                  this.props.targetWords,
                  child.highlightWords
                )}
                onClick={this.props.onClick}
                key={index}
              />
            ))
          : TEST_DATA.map((child, index) => (
              <ItemComponent
                item={child}
                listMode={this.props.listMode}
                activeSearch={this.props.activeSearch}
                targetWords={this.injectHighlightWords(
                  this.props.targetWords,
                  child.highlightWords
                )}
                onClick={this.props.onClick}
                key={index}
              />
            ))}
      </div>
    );
  }
}
