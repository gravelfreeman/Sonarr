import React, { useCallback, useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import Alert from 'Components/Alert';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { kinds } from 'Helpers/Props';
import { fetchRootFolders } from 'Store/Actions/rootFolderActions';
import createRootFoldersSelector from 'Store/Selectors/createRootFoldersSelector';
import { InputOnChange } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import RootFolderRow from './RootFolderRow';

const rootFolderColumns = [
  {
    name: 'path',
    label: () => translate('Path'),
    isVisible: true,
  },
  {
    name: 'freeSpace',
    label: () => translate('FreeSpace'),
    isVisible: true,
  },
  {
    name: 'unmappedFolders',
    label: () => translate('UnmappedFolders'),
    isVisible: true,
  },
  {
    name: 'recycleBinEnabled',
    label: () => translate('RecyclingBin'),
    isVisible: true,
  },
  {
    name: 'actions',
    isVisible: true,
  },
];

type RootFolderUpdate = {
  id: number;
  recycleBinEnabled: boolean;
};

interface RootFoldersProps {
  rootFolderUpdates?: RootFolderUpdate[];
  onInputChange?: InputOnChange<RootFolderUpdate[]>;
}

function RootFolders(props: RootFoldersProps) {
  const { rootFolderUpdates = [], onInputChange } = props;
  const { isFetching, isPopulated, error, items } = useSelector(
    createRootFoldersSelector()
  );

  const recycleBinEnabledPending = rootFolderUpdates.reduce(
    (result, update) => {
      result[update.id] = update.recycleBinEnabled;
      return result;
    },
    {} as Record<number, boolean>
  );

  const onRecycleBinChange = useCallback(
    (
      id: number,
      recycleBinEnabledPending: boolean,
      recycleBinEnabled: boolean
    ) => {
      const updates = rootFolderUpdates.filter((update) => update.id !== id);

      if (recycleBinEnabledPending !== recycleBinEnabled) {
        updates.push({ id, recycleBinEnabled: recycleBinEnabledPending });
      }

      onInputChange?.({ name: 'rootFolderUpdates', value: updates });
    },
    [onInputChange, rootFolderUpdates]
  );

  const dispatch = useDispatch();

  useEffect(() => {
    dispatch(fetchRootFolders());
  }, [dispatch]);

  if (isFetching && !isPopulated) {
    return <LoadingIndicator />;
  }

  if (!isFetching && !!error) {
    return (
      <Alert kind={kinds.DANGER}>{translate('RootFoldersLoadError')}</Alert>
    );
  }

  return (
    <Table columns={rootFolderColumns}>
      <TableBody>
        {items.map((rootFolder) => {
          return (
            <RootFolderRow
              key={rootFolder.id}
              id={rootFolder.id}
              path={rootFolder.path}
              recycleBinEnabled={rootFolder.recycleBinEnabled}
              recycleBinEnabledPending={
                recycleBinEnabledPending[rootFolder.id] ??
                rootFolder.recycleBinEnabled
              }
              accessible={rootFolder.accessible}
              freeSpace={rootFolder.freeSpace}
              unmappedFolders={rootFolder.unmappedFolders}
              onRecycleBinChange={onRecycleBinChange}
            />
          );
        })}
      </TableBody>
    </Table>
  );
}

export default RootFolders;
