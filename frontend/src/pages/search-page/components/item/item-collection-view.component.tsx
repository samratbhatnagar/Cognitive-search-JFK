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
    extraFields:
      "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris tincidunt, nisi ut tempor luctus, ligula elit dictum metus.",
    metadata: ["test", "data"],
    demoInitialPage: 0,
    type: "json",
    highlightWords: ["Hello", "Sir"]
  },
  {
    title: "test title 2",
    subtitle: "",
    thumbnail: "",
    excerpt: "",
    rating: 0,
    extraFields: { Name: "Abc", Age: 15, Location: "Bangalore" },
    metadata: ["test2", "data2", "more", "information"],
    demoInitialPage: 0,
    type: "json",
    highlightWords: ["Hello", "Sir"]
  },
  {
    title: "test title 3",
    subtitle: "",
    thumbnail: "",
    excerpt: "",
    rating: 0,
    extraFields: {
      id: 1,
      name: "node1",
      children: [{ name: "child1", id: 2 }, { name: "child2", id: 3 }]
    },
    metadata: ["test3", "data3", "more", "information", "third"],
    demoInitialPage: 0,
    type: "json",
    highlightWords: ["Hello", "Sir"]
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
